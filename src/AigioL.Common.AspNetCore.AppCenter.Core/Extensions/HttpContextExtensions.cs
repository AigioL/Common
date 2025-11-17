using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Primitives;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using static AigioL.Common.AspNetCore.AppCenter.Policies.Handlers.IJsonWebTokenAuthorizationMiddlewareResultHandler;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Http;

public static partial class HttpContextExtensions
{
    /// <summary>
    /// 从 HTTP 上下文中获取用户的 JWT ID
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid? GetJwtUserId(this HttpContext context)
    {
        if (context.Items.TryGetValue(KEY_USER_JWT_ID, out var jwtUserIdObj))
        {
            if (jwtUserIdObj is Guid jwtUserId)
            {
                return jwtUserId;
            }
        }
        return null;
    }

    /// <summary>
    /// 从 HTTP 上下文中获取用户的 JWT ID
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid GetJwtUserIdThrowIfNull(this HttpContext context)
    {
        var jwtUserId = context.GetJwtUserId();
        if (!jwtUserId.HasValue)
#pragma warning disable CA2208 // 正确实例化参数异常
            throw new ArgumentNullException(nameof(jwtUserId));
#pragma warning restore CA2208 // 正确实例化参数异常
        return jwtUserId.Value;
    }

    /// <summary>
    /// 从 HTTP 上下文中获取用户的 ID
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid? GetUserId(this HttpContext context)
    {
        if (context.Items.TryGetValue(KEY_USER_ID, out var userIdObj))
        {
            if (userIdObj is Guid userId)
            {
                return userId;
            }
        }
        return null;
    }

    /// <summary>
    /// 从 HTTP 上下文中获取用户的 ID
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid GetUserIdThrowIfNull(this HttpContext context)
    {
        var userId = context.GetUserId();
        if (!userId.HasValue)
#pragma warning disable CA2208 // 正确实例化参数异常
            throw new ArgumentNullException(nameof(userId));
#pragma warning restore CA2208 // 正确实例化参数异常
        return userId.Value;
    }

    const string KEY_APP_VERSION = "KEY_APP_VERSION";

    /// <summary>
    /// 从 HTTP 上下文中获取客户端应用程序的版本信息
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task<IReadOnlyAppVer?> GetAppVerAsync(this HttpContext context)
    {
        if (context.Items.TryGetValue(KEY_APP_VERSION, out var appVerObj))
        {
            if (appVerObj is IReadOnlyAppVer appVer)
            {
                // 从 HTTP 上下文缓存中获取
                return appVer;
            }
        }

        // 从服务接口中获取
        {
            var appVerCoreService = context.RequestServices.GetRequiredService<IAppVerCoreService>();
            var appVer = await appVerCoreService.GetAsync(context, fromHeaderOrQuery: true);
            return appVer;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetTraceId(this HttpContext context)
    {
        // https://github.com/dotnet/aspnetcore/blob/v9.0.8/src/Http/Http.Extensions/src/DefaultProblemDetailsWriter.cs#L58
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        return traceId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetExceptionHandlerPath(this HttpContext context, out Exception? error)
    {
        error = null;
        string path;
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature != null)
        {
            path = exceptionHandlerPathFeature.Path;
            error = exceptionHandlerPathFeature.Error;
        }
        else
        {
            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (exceptionHandlerFeature != null)
            {
                path = exceptionHandlerFeature.Path;
                error = exceptionHandlerFeature.Error;
            }
            else
            {
                path = context.Request.Path;
            }
        }
        return path;
    }

    internal static DevicePlatform2? GetDevicePlatformCore(ReadOnlySpan<char> chars)
    {
        var index = chars.IndexOf(':');
        if (index > 0)
        {
            var enumChars = chars[(index + 1)..].Trim('/');
            index = enumChars.IndexOf('/');
            if (index > 0)
            {
                enumChars = enumChars[..index];
                if (Enum.TryParse<DevicePlatform2>(enumChars, ignoreCase: true, out var platform))
                {
                    return platform;
                }
                else if (enumChars.Equals("Windows Desktop Bridge", StringComparison.InvariantCultureIgnoreCase))
                {
                    return DevicePlatform2.WindowsDesktopBridge;
                }
            }
        }
        return default;
    }

    public static StringValues GetCustomReferrer(this IHeaderDictionary headers)
    {
        StringValues referrer = headers["Referrer"];
        if (StringValues.IsNullOrEmpty(referrer))
        {
            referrer = headers["Referer2"];
            if (StringValues.IsNullOrEmpty(referrer))
            {
                referrer = headers.Referer;
            }
        }
        return referrer;
    }

    static DevicePlatform2 GetDevicePlatformCore(this HttpRequest request)
    {
        ReadOnlySpan<char> chars = default;

        var referer = request.Headers.GetCustomReferrer();
        if (!StringValues.IsNullOrEmpty(referer))
        {
            chars = referer.ToString().AsSpan();
        }

        try
        {
            if (!chars.IsWhiteSpace())
            {
                var platform = GetDevicePlatformCore(chars);
                if (platform.HasValue)
                {
                    return platform.Value;
                }
            }
        }
        catch
        {
        }

        // 备选方案：通过 User-Agent 头部信息进行简单判断
        var userAgent = request.Headers.UserAgent;
        if (!StringValues.IsNullOrEmpty(userAgent))
        {
            if (userAgent.Contains("Android"))
            {
                return DevicePlatform2.AndroidPhone;
            }
            if (userAgent.Contains("Windows NT"))
            {
                return DevicePlatform2.Windows;
            }
            if (userAgent.Contains("Intel Mac OS X"))
            {
                return DevicePlatform2.macOS;
            }
            if (userAgent.Contains("iPad"))
            {
                return DevicePlatform2.iPadOS;
            }
            if (userAgent.Contains("iPhone"))
            {
                return DevicePlatform2.iOS;
            }
            if (userAgent.Contains("Linux"))
            {
                return DevicePlatform2.Linux;
            }
        }

        return default;
    }

    /// <summary>
    /// 从 HTTP 请求中获取客户端平台
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DevicePlatform2 GetDevicePlatform(this HttpRequest request)
    {
        const string key = "DevicePlatform2";

        if (request.HttpContext.Items.TryGetValue(key, out var value) &&
           value is DevicePlatform2 platform)
        {
            return platform;
        }

        platform = GetDevicePlatformCore(request);
        request.HttpContext.Items[key] = platform;
        return platform;
    }

    /// <summary>
    /// 从 HTTP 上下文中获取客户端平台
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DevicePlatform2 GetDevicePlatform(this HttpContext context)
        => context.Request.GetDevicePlatform();

    /// <summary>
    /// 从 HTTP 请求 Query 中获取枚举
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="request"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TEnum[]? GetQueryEnums<TEnum>(this HttpRequest request, string key)
        where TEnum : struct, Enum
    {
        if (request.Query.TryGetValue(key, out var value))
        {
            if (value.Count > 0)
            {
                var result = new TEnum[value.Count];
                int arrIndex = 0;
                for (int i = 0; i < value.Count; i++)
                {
                    var it = value[i];
                    if (Enum.TryParse<TEnum>(it, true, out var @enum))
                    {
                        result[arrIndex++] = @enum;
                    }
                }
                if (arrIndex == result.Length - 1)
                {
                    return result;
                }
                else
                {
                    var resultLocal = new TEnum[arrIndex + 1];
                    Array.Copy(result, resultLocal, resultLocal.Length);
                    return resultLocal;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 从 HTTP 上下文的请求 Query 中获取枚举
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="context"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TEnum[]? GetQueryEnums<TEnum>(this HttpContext context, string key) where TEnum : struct, Enum => context.Request.GetQueryEnums<TEnum>(key);

    /// <summary>
    /// 从 HTTP 请求 Query 中获取日期时间范围
    /// </summary>
    /// <param name="request"></param>
    /// <param name="key"></param>
    /// <param name="fixedLength"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset[]? GetQueryDateTimeRange(this HttpRequest request, string key, int? fixedLength = 2)
    {
        if (request.Query.TryGetValue(key, out var value))
        {
            if (value.Count > 0)
            {
                if (!fixedLength.HasValue || (value.Count >= fixedLength.Value))
                {
                    var result = new DateTimeOffset[fixedLength.HasValue ? fixedLength.Value : value.Count];
                    int arrIndex = 0;
                    for (int i = 0; i < value.Count; i++)
                    {
                        var it = value[i];
                        if (DateTimeParseHelper.TryParse(it, out var result1))
                        {
                            result[arrIndex++] = result1.Value;
                            if (fixedLength.HasValue)
                            {
                                if (fixedLength.Value == arrIndex)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    if (arrIndex == result.Length - 1)
                    {
                        return result;
                    }
                    else
                    {
                        var resultLocal = new DateTimeOffset[arrIndex + 1];
                        Array.Copy(result, resultLocal, resultLocal.Length);
                        return resultLocal;
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 从 HTTP 上下文的请求 Query 中获取日期时间范围
    /// </summary>
    /// <param name="context"></param>
    /// <param name="key"></param>
    /// <param name="fixedLength"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTimeOffset[]? GetQueryDateTimeRange(this HttpContext context, string key, int? fixedLength = 2) => context.Request.GetQueryDateTimeRange(key, fixedLength);

    /// <summary>
    /// 获取客户端 IP 地址
    /// </summary>
    /// <param name="context"></param>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetRemoteIpAddress(this HttpContext context, [NotNullWhen(true)] out string? ipAddress)
    {
        ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrWhiteSpace(ipAddress))
        {
            if (IPAddress.TryParse(ipAddress, out _))
            {
                return true;
            }
        }
        return false;
    }
}

file static partial class DateTimeParseHelper
{
    #region https://github.com/dotnet/runtime/blob/v9.0.8/src/libraries/System.Text.Json/src/System/Text/Json/JsonConstants.cs

    // In the worst case, an ASCII character represented as a single utf-8 byte could expand 6x when escaped.
    // For example: '+' becomes '\u0043'
    // Escaping surrogate pairs (represented by 3 or 4 utf-8 bytes) would expand to 12 bytes (which is still <= 6x).
    // The same factor applies to utf-16 characters.
    public const int MaxExpansionFactorWhileEscaping = 6;

    public const int MaximumFormatDateTimeOffsetLength = 33;  // StandardFormat 'O', e.g. 2017-06-12T05:30:45.7680000-07:00
    public const int DateTimeParseNumFractionDigits = 16; // The maximum number of fraction digits the Json DateTime parser allows
    public const int DateTimeNumFractionDigits = 7;  // TimeSpan and DateTime formats allow exactly up to many digits for specifying the fraction after the seconds.
    public const int MaximumDateTimeOffsetParseLength = (MaximumFormatDateTimeOffsetLength +
            (DateTimeParseNumFractionDigits - DateTimeNumFractionDigits)); // Like StandardFormat 'O' for DateTimeOffset, but allowing 9 additional (up to 16) fraction digits.
    public const int MinimumDateTimeParseLength = 10; // YYYY-MM-DD
    public const int MaximumEscapedDateTimeOffsetParseLength = MaxExpansionFactorWhileEscaping * MaximumDateTimeOffsetParseLength;

    #endregion

    public static bool TryParse(string? s, [NotNullWhen(true)] out DateTimeOffset? result)
    {
        if (!string.IsNullOrWhiteSpace(s))
        {
            if (DateTimeOffset.TryParseExact(s, "r", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces, out var value1))
            {
                // Microsoft.AspNetCore.Http.RequestDelegateGenerator 源生成
                // if (global::System.DateTimeOffset.TryParse(status3_temp!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces, out var status3_temp_parsed_non_nullable))
                result = value1;
                return true;
            }
            else if (s.Length <= MaximumEscapedDateTimeOffsetParseLength)
            {
                // https://learn.microsoft.com/zh-cn/dotnet/standard/datetime/system-text-json-support#support-for-the-iso-8601-12019-format
                Span<byte> buffer = stackalloc byte[Encoding.UTF8.GetMaxByteCount(MaximumEscapedDateTimeOffsetParseLength + 2)];
                var yh = "\""u8;
                yh.CopyTo(buffer);
                var temp = buffer[yh.Length..];
                Encoding.UTF8.TryGetBytes(s, temp, out var bytesWritten);
                temp = temp[bytesWritten..];
                yh.CopyTo(temp);
                buffer = buffer[(bytesWritten + yh.Length * 2)..];
                Utf8JsonReader reader = new(buffer);
                if (reader.TryGetDateTimeOffset(out var value2))
                {
                    result = value2;
                    return true;
                }
            }
        }

        result = null;
        return false;
    }
}