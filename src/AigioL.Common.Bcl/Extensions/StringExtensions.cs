using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System;

public static partial class StringExtensions
{
    /// <summary>
    /// https://
    /// </summary>
    public const string Prefix_HTTPS = "https://";

    /// <summary>
    /// http://
    /// </summary>
    public const string Prefix_HTTP = "http://";

    /// <summary>
    /// 判断字符串是否为 HttpUrl
    /// </summary>
    /// <param name="str"></param>
    /// <param name="httpsOnly">是否仅Https</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHttpUrl([NotNullWhen(true)] this string? str, bool httpsOnly = false) => str != null &&
        (str.StartsWith(Prefix_HTTPS, StringComparison.OrdinalIgnoreCase) ||
              (!httpsOnly && str.StartsWith(Prefix_HTTP, StringComparison.InvariantCultureIgnoreCase)));

    /// <summary>
    /// 判断字符串是否为邮箱地址
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmail(this string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return false;
        }

        // https://github.com/dotnet/runtime/blob/v10.0.0-rc.1.25451.107/src/libraries/System.ComponentModel.Annotations/src/System/ComponentModel/DataAnnotations/EmailAddressAttribute.cs#L18

        if (str.AsSpan().ContainsAny('\r', '\n'))
        {
            return false;
        }

        // only return true if there is only 1 '@' character
        // and it is neither the first nor the last character
        int index = str.IndexOf('@');

        return
            index > 0 &&
            index != str.Length - 1 &&
            index == str.LastIndexOf('@');
    }

    /// <inheritdoc cref="string.Format(string, object?[])"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Format(this string format, params object?[] args)
    {
        try
        {
            return string.Format(format, args);
        }
        catch
        {
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return string.Join(' ', new[] { format }.Concat(args));
#else
            return string.Join(" ", new[] { format }.Concat(args));
#endif
        }
    }

    public static bool TryGetVersion(this ReadOnlySpan<char> s, [NotNullWhen(true)] out Version? version)
    {
        const string winosvp = "Microsoft Windows NT ";
        if (s.StartsWith(winosvp))
        {
            s = s[winosvp.Length..];
        }
        return Version.TryParse(s, out version);
    }

    /// <summary>
    /// 将字符串解析为 <see cref="DateTimeOffset"/>，使用 <see cref="Utf8JsonReader"/> 解析 ISO 8601-1:2019 格式的日期时间字符串
    /// </summary>
    /// <param name="s"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryGetDateTimeOffset(this ReadOnlySpan<char> s, [NotNullWhen(true)] out DateTimeOffset? result)
    {
        // DateTime.UtcNow.ToString("o").Length = 28 | GetMaxByteCount 87
        if (s.Length > 8 && s.Length <= 28)
        {
            // https://learn.microsoft.com/zh-cn/dotnet/standard/datetime/system-text-json-support#support-for-the-iso-8601-12019-format
            Span<byte> buffer = stackalloc byte[93]; // GetMaxByteCount 28+2
            var yh = "\""u8;
            yh.CopyTo(buffer);

            var temp = buffer[yh.Length..];
            Encoding.UTF8.TryGetBytes(s, temp, out var bytesWritten);

            temp = temp[bytesWritten..];
            yh.CopyTo(temp);

            buffer = buffer[..(bytesWritten + yh.Length * 2)];
            Utf8JsonReader reader = new(buffer);
            reader.Read();
            if (reader.TryGetDateTimeOffset(out var value2))
            {
                result = value2;
                return true;
            }
        }

        result = null;
        return false;
    }

    /// <summary>
    /// 将字符串解析为 <see cref="DateTime"/>，使用 <see cref="Utf8JsonReader"/> 解析 ISO 8601-1:2019 格式的日期时间字符串
    /// </summary>
    /// <param name="s"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryGetDateTime(this ReadOnlySpan<char> s, [NotNullWhen(true)] out DateTime? result)
    {
        // DateTime.UtcNow.ToString("o").Length = 28 | GetMaxByteCount 87
        if (s.Length > 8 && s.Length <= 28)
        {
            // https://learn.microsoft.com/zh-cn/dotnet/standard/datetime/system-text-json-support#support-for-the-iso-8601-12019-format
            Span<byte> buffer = stackalloc byte[93]; // GetMaxByteCount 28+2
            var yh = "\""u8;
            yh.CopyTo(buffer);

            var temp = buffer[yh.Length..];
            Encoding.UTF8.TryGetBytes(s, temp, out var bytesWritten);

            temp = temp[bytesWritten..];
            yh.CopyTo(temp);

            buffer = buffer[..(bytesWritten + yh.Length * 2)];
            Utf8JsonReader reader = new(buffer);
            reader.Read();
            if (reader.TryGetDateTime(out var value2))
            {
                result = value2;
                return true;
            }
        }

        result = null;
        return false;
    }
}