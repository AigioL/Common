using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Response;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using AigioL.Common.Models;
using MemoryPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IO;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using static AigioL.Common.AspNetCore.AppCenter.Identity.Controllers.D3b96193;
using static AigioL.Common.AspNetCore.AppCenter.Identity.Controllers.ErrorController;
using static AigioL.Common.AspNetCore.AppCenter.Identity.Controllers.FixSession;
using AlipayConstants = AspNet.Security.OAuth.Alipay.AlipayAuthenticationConstants2;
using QQConstants = AspNet.Security.OAuth.QQ.QQAuthenticationConstants;
using R = AigioL.Common.AspNetCore.AppCenter.Identity.UI.Properties.Resources;
using WeChatConstants = AspNet.Security.OAuth.Weixin.WeixinAuthenticationConstants;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

/// <summary>
/// 第三方外部账号登录终结点
/// </summary>
public static partial class ExternalLoginController
{
    public static void MapIdentityExternalLoginV5<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v5/externallogin")
        where TAppSettings : class, IExternalLoginRedirect
    {
        RoutePattern = pattern;
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapGet("{channel?}", async (HttpContext context,
            [FromRoute] ExternalLoginChannel channel = DefaultExternalLoginChannel) =>
        {
            var r = await ExternalLoginDetectionAsync(context, channel);
            return r;
        }).WithIdentityUIView()
        .WithDescription("第三方外部平台登录接口（第一步：浏览器兼容性检测）");
        routeGroup.MapGet("step2/{channel?}", async (HttpContext context,
            [FromRoute] ExternalLoginChannel channel = DefaultExternalLoginChannel) =>
        {
            var r = await ExternalLogin(context, channel);
            return r;
        }).WithIdentityUIView()
        .WithDescription("第三方外部平台登录接口（第二步：跳转外部网站进行授权）");
        routeGroup.MapGet("step3", async (HttpContext context) =>
        {
            var r = await Callback<TAppSettings>(context);
            return r;
        }).WithIdentityUIView()
        .WithDescription("第三方外部平台登录接口（第三步：由外部网站回调此接口）");

#if DEBUG
        routeGroup.MapGet("test/ex", async (HttpContext context) =>
        {
            await Task.Delay(10, context.RequestAborted);
            throw new ApplicationException("测试 WithIdentityUIView 的异常页面");
        }).WithIdentityUIView();
#endif
    }

    /// <summary>
    /// 第三方外部平台登录接口（第一步：浏览器兼容性检测）
    /// </summary>
    static Task<IResult> ExternalLoginDetectionAsync(
        HttpContext context,
        ExternalLoginChannel channel)
    {
        ResponseCacheNone(context);
        context.Session.Clear();
        SetQueryToSession(context);
        return ExternalLoginDetectionCoreAsync(context, channel: channel);
    }

    /// <summary>
    /// 第三方外部平台登录接口（第二步：跳转外部网站进行授权）
    /// </summary>
    static async Task<IResult> ExternalLogin(
        HttpContext context,
        ExternalLoginChannel channel)
    {
        ResponseCacheNone(context);

        if (!Enum.IsDefined(channel))
        {
            return Results.NotFound();
        }
        SetSessionInt32(context, nameof(ExternalLoginChannel), unchecked((int)channel));

        var appVerCoreService = context.RequestServices.GetRequiredService<IAppVerCoreService>();
        var (errMsg, _) = await appVerCoreService.GetSecurityResultAsync(context);
        if (errMsg != default)
        {
            var r = await ExternalLoginDetectionCoreAsync(context, error: errMsg, channel: channel);
            return r;
        }

        var userId = context.GetUserId();
        var isBind = bool.TryParse(GetSessionString(context, KeyIsBind), out var isBind_) && isBind_;
        context.Items[KeyIsBind] = isBind;
        if (isBind)
        {
            if (!userId.HasValue)
            {
                // 绑定账号时，用户不能是未登录
                var r = await ExternalLoginDetectionCoreAsync(context, error: R.IsBindTrueUserIsNull, channel: channel);
                return r;
            }
        }
        else
        {
            if (userId.HasValue)
            {
                // 非绑定账号时，用户不能是已登录
                var r = await ExternalLoginDetectionCoreAsync(context, error: R.IsBindFalseUserIsNotNull, channel: channel);
                return r;
            }
        }

        var pattern = RoutePattern;
        ArgumentNullException.ThrowIfNull(pattern);
        var redirectUrl = $"/{pattern.AsSpan().TrimStart('/').TrimEnd('/')}/step3";
        var provider = channel.ToString2();

        var signInManager = context.RequestServices.GetRequiredService<SignInManager<User>>();
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Results.Challenge(properties, [provider]);
    }

    /// <summary>
    /// 第三方外部平台登录接口（第三步：由外部网站回调此接口）
    /// </summary>
    static async Task<IResult> Callback<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        HttpContext context)
        where TAppSettings : class, IExternalLoginRedirect
    {
        ResponseCacheNone(context);

        var channelInt32 = GetSessionInt32(context, nameof(ExternalLoginChannel));
        var channel = channelInt32.HasValue ? (ExternalLoginChannel)channelInt32.Value : DefaultExternalLoginChannel;

        string? deviceId = null;
        if (ShortGuid.TryParse(GetSessionString(context, "dg"), out Guid dg))
        {
            deviceId = new DeviceId(dg,
                GetSessionString(context, "dr"),
                GetSessionString(context, "dn"))
                .GetDeviceId();
        }

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            var r = await ExternalLoginDetectionCoreAsync(context, error: "unknown device id g.", channel: channel);
            return r;
        }

        var remoteError = context.Request.Query["remoteError"];
        var port = GetSessionString(context, "port");
        var isWeb = bool.TryParse(GetSessionString(context, "isWeb"), out var isWebB) && isWebB;
        var portHasValue = !string.IsNullOrEmpty(port);
        if (portHasValue && !ushort.TryParse(port, out var _))
        {
            var r = await ExternalLoginDetectionCoreAsync(context, error: R.WebSocketPortIsNotUInt16, channel: channel);
            return r;
        }
        if (!StringValues.IsNullOrEmpty(remoteError))
        {
            var javaScriptEncoder = context.RequestServices.GetService<JavaScriptEncoder>() ?? JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            var remoteErrorEncode = javaScriptEncoder.Encode(remoteError!);
            var r = await ExternalLoginDetectionCoreAsync(context, error: R.ErrorFromExternalProvider_.Format(remoteErrorEncode), channel: channel);
            return r;
        }

        var appVerCoreService = context.RequestServices.GetRequiredService<IAppVerCoreService>();
        var (errMsg, aes) = await appVerCoreService.GetSecurityResultAsync(context);
        if (errMsg != default)
        {
            var r = await ExternalLoginDetectionCoreAsync(context, error: errMsg, channel: channel);
            return r;
        }

        ArgumentNullException.ThrowIfNull(aes);

        var userId = context.GetUserId();
        var isBind = bool.TryParse(GetSessionString(context, KeyIsBind), out var isBind_) && isBind_;
        context.Items[KeyIsBind] = isBind;
        if (isBind)
        {
            if (!userId.HasValue)
            {
                // 绑定账号时，用户不能是未登录
                var r = await ExternalLoginDetectionCoreAsync(context, error: R.IsBindTrueUserIsNull, channel: channel);
                return r;
            }
        }
        else
        {
            if (userId.HasValue)
            {
                // 非绑定账号时，用户不能是已登录
                var r = await ExternalLoginDetectionCoreAsync(context, error: R.IsBindFalseUserIsNotNull, channel: channel);
                return r;
            }
        }

        var signInManager = context.RequestServices.GetRequiredService<SignInManager<User>>();
        var externalLoginInfo = await signInManager.GetExternalLoginInfoAsync();
        if (externalLoginInfo == null)
        {

            var r = await ExternalLoginDetectionCoreAsync(context, error: R.ErrorLoadingExternalLoginInfo, channel: channel);
            return r;
        }
        await signInManager.SignOutAsync(); // 登出第三方账号，重新授权本平台 JWT

        var authType = externalLoginInfo.Principal.Identity?.AuthenticationType;
        if (!Enum.TryParse<ExternalLoginChannel>(authType, true, out var channel2))
        {
            var r = await ExternalLoginDetectionCoreAsync(context, error: R.ErrorFromUnknownAuthType_.Format(authType), channel: channel);
            return r;
        }
        channel = channel2;

        var userManager = context.RequestServices.GetRequiredService<IUserManager2>();
        ApiRsp<LoginOrRegisterResponse?> rsp = null!;
        switch (channel)
        {
            case ExternalLoginChannel.Steam:
                {
                    var steamNameIdentifier = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrWhiteSpace(steamNameIdentifier))
                    {
                        var r = await ExternalLoginDetectionCoreAsync(context, error: R.ReadFailBySteamAccountId64_.Format("null"), channel: channel);
                        return r;
                    }
                    var steamAccountId64 = GetSteamAccountId64(steamNameIdentifier);
                    if (!steamAccountId64.HasValue)
                    {
                        var r = await ExternalLoginDetectionCoreAsync(context, error: R.ReadFailBySteamAccountId64_.Format(steamNameIdentifier), channel: channel);
                        return r;
                    }
                    var name = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Name);
                    rsp = await userManager.LoginOrRegisterOrBindAsync(steamAccountId64.Value.ToString(), channel, deviceId, userId, p =>
                    {
                        p.NickName = name;
                    });
                }
                break;
            case ExternalLoginChannel.Microsoft:
                {
                    var nameIdentifier = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrWhiteSpace(nameIdentifier))
                    {
                        var r = await ExternalLoginDetectionCoreAsync(context, error: R.ClaimTypes_NameIdentifier_IsNull__.Format("null", channel), channel: channel);
                        return r;
                    }
                    var name = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Name);
                    var email = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email);
                    var givenName = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.GivenName);
                    var surname = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Surname);
                    rsp = await userManager.LoginOrRegisterOrBindAsync(nameIdentifier, channel, deviceId, userId, x =>
                    {
                        x.NickName = name;
                        x.Email = email;
                        x.GivenName = givenName;
                        x.Surname = surname;
                    });
                }
                break;
            case ExternalLoginChannel.QQ:
                {
                    var unionId = externalLoginInfo.Principal.FindFirstValue(QQConstants.Claims.UnionId);
                    if (string.IsNullOrWhiteSpace(unionId))
                    {
                        var r = await ExternalLoginDetectionCoreAsync(context, error: R.ReadFailByUnionId_.Format(unionId), channel: channel);
                        return r;
                    }
                    var nickname = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Name);
                    var gender = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Gender);
                    var avatar_full = externalLoginInfo.Principal.FindFirstValue(QQConstants.Claims.AvatarFullUrl);
                    rsp = await userManager.LoginOrRegisterOrBindAsync(unionId, channel, deviceId, userId, x =>
                    {
                        x.NickName = nickname;
                        x.Gender = ExternalAccount.ParseGenderStr(gender);
                        x.AvatarUrl = avatar_full;
                    });
                }
                break;
            case ExternalLoginChannel.Weixin:
                {
                    var openId = externalLoginInfo.Principal.FindFirstValue(WeChatConstants.Claims.OpenId);
                    if (string.IsNullOrWhiteSpace(openId))
                    {
                        var r = await ExternalLoginDetectionCoreAsync(context, error: R.ReadFailByOpenId_.Format(openId), channel: channel);
                        return r;
                    }
                    var nickname = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Name);
                    var gender = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Gender);
                    var avatar_full = externalLoginInfo.Principal.FindFirstValue(WeChatConstants.Claims.HeadImgUrl);
                    rsp = await userManager.LoginOrRegisterOrBindAsync(openId, channel, deviceId, userId, x =>
                    {
                        x.NickName = nickname;
                        x.Gender = ExternalAccount.ParseGenderStr(gender);
                        x.AvatarUrl = avatar_full;
                    });
                }
                break;
            case ExternalLoginChannel.Alipay:
                {
                    var openId = externalLoginInfo.Principal.FindFirstValue(AlipayConstants.Claims.OpenId);
                    if (string.IsNullOrWhiteSpace(openId))
                    {
                        var r = await ExternalLoginDetectionCoreAsync(context, error: R.ReadFailByOpenId_.Format(openId), channel: channel);
                        return r;
                    }
                    var nickname = externalLoginInfo.Principal.FindFirstValue(AlipayConstants.Claims.Nickname);
                    var gender = externalLoginInfo.Principal.FindFirstValue(AlipayConstants.Claims.Gender);
                    var avatar_full = externalLoginInfo.Principal.FindFirstValue(AlipayConstants.Claims.Avatar);
                    rsp = await userManager.LoginOrRegisterOrBindAsync(openId, channel, deviceId, userId, x =>
                    {
                        x.NickName = nickname;
                        x.Gender = ExternalAccount.ParseGenderStr(gender);
                        x.AvatarUrl = avatar_full;
                    });
                }
                break;
            //case ExternalLoginChannel.Apple:
            //    {
            //        // TODO: xxx
            //    }
            //    break;
            //case ExternalLoginChannel.Facebook:
            //    break;
            //case ExternalLoginChannel.Twitter:
            //    break;
            //case ExternalLoginChannel.Google:
            //    break;
            //case ExternalLoginChannel.Feishu:
            //    break;
            default:
                {
                    var r = await ExternalLoginDetectionCoreAsync(context, error: R.ErrorFromUnknownChannel_.Format(channel), channel: channel);
                    return r;
                }
        }

        // 返回页面 Html
        async Task<IResult> EndPageAsync(string? error, string? token)
        {
            var useUrlSchemeLoginToken = bool.TryParse(GetSessionString(context, "isUS"), out var isUS) && isUS;
            var r = await ExternalLoginDetectionCoreAsync(
                context, error: error, token: token,
                port: port, useUrlSchemeLoginToken: useUrlSchemeLoginToken, channel: channel);
            return r;
        }

        // 返回错误消息的页面 Html
        Task<IResult> EndErrorPageAsync(ApiRsp apiRsp)
        {
            var error = apiRsp.Message;
            if (string.IsNullOrWhiteSpace(error))
            {
                error = R.ExternalLoginError_.Format(unchecked((int)apiRsp.Code));
            }
            var r = EndPageAsync(error, null);
            return r;
        }

        if (!portHasValue)
        {
            if (isWeb)
            {
                if (isBind)
                {
                    if (rsp.IsSuccess())
                    {
                        var options = context.RequestServices.GetRequiredService<IOptions<TAppSettings>>();
                        var url = options.Value.AccountCenterBindUrl;
                        if (!url.IsHttpUrl(context.Request.IsHttps))
                        {
                            url = "/account/center/bind";
                        }
                        return Results.Redirect(url);
                    }
                    else
                    {
                        var r = await EndErrorPageAsync(rsp);
                        return r;
                    }
                }
                else
                {
                    if (rsp.IsSuccess() && rsp.Content?.AuthToken != null)
                    {
                        var redirectUrl = GetSessionString(context, "redirectUrl");
                        var isOAuth = string.IsNullOrWhiteSpace(redirectUrl);
                        if (isOAuth)
                        {
                            // OAuth 使用 ReturnUrl
                            redirectUrl = GetSessionString(context, "ReturnUrl");
                        }
                        redirectUrl = WebUtility.UrlEncode(redirectUrl);
                        var ticks = rsp.Content.AuthToken.ExpiresIn.Ticks;
                        var options = context.RequestServices.GetRequiredService<IOptions<TAppSettings>>();
                        var url = options.Value.AccountLoginUrl ?? "/account/login";
                        // TODO: 改为 QueryHelpers.AddQueryString(string uri, IDictionary<string, string?> queryString)
                        url = $"{url.AsSpan().TrimEnd('/')}?tk={rsp.Content.AuthToken.AccessToken}&t={ticks}&isBind={isBind}{(string.IsNullOrWhiteSpace(redirectUrl) ? "" : isOAuth ? $"&ReturnUrl={redirectUrl}" : $"&redirectUrl={redirectUrl}")}";
                        return Results.Redirect(url);
                    }
                    else
                    {
                        var r = await EndErrorPageAsync(rsp);
                        return r;
                    }
                }
            }
            else
            {
                bool isMemoryPack = false;
                if (isMemoryPack)
                {
                    var encryptStream = await SerializeEncryptAsync(rsp, aes, context.RequestAborted);
                    return Results.File(encryptStream, MediaTypeNames.MemoryPackSecurity);
                }
                else
                {
                    rsp.Content?.FastLRBChannel = channel;
                    if (rsp.IsSuccess())
                    {
                        var token = await SerializeEncryptToJsonAsync(rsp, aes, context.RequestAborted);
                        var r = await EndPageAsync(null, token);
                        return r;
                    }
                    else
                    {
                        var r = await EndErrorPageAsync(rsp);
                        return r;
                    }
                }
            }
        }
        else
        {
            rsp.Content?.FastLRBChannel = channel;
            if (rsp.IsSuccess())
            {
                using var encryptStream = await SerializeEncryptAsync(rsp, aes, context.RequestAborted);
                var rspStr = Base64Url.EncodeToString(encryptStream.GetBuffer().AsSpan()[..unchecked((int)encryptStream.Length)]);
                var r = await EndPageAsync(null, rspStr);
                return r;
            }
            else
            {
                var r = await EndErrorPageAsync(rsp);
                return r;
            }
        }
    }
}

file static class D3b96193
{
    internal static string? RoutePattern { get; set; }

    internal static readonly string[] QueryKeys = [
        "port",
        "sKeyHex",
        "sKeyPadding",
        "ver",
        "isBind",
        "access_token_expires_hex",
        "access_token_hex",
        "access_token",
        "access_token_expires",
        "isWeb",
        "redirectUrl",
        "ReturnUrl",
        "isUS", // 是否使用 UrlScheme
        "dg", // DeviceIdG
        "dr", // DeviceIdR
        "dn", // DeviceIdN
    ];

    internal const string KeyIsBind = "isBind";

    internal const ExternalLoginChannel DefaultExternalLoginChannel = ExternalLoginChannel.Steam;

    internal static string ToString2(this ExternalLoginChannel channel) => channel switch
    {
        ExternalLoginChannel.Xbox => nameof(ExternalLoginChannel.Microsoft),
        _ => channel.ToString(),
    };

    /// <summary>
    /// 返回 HTML 页面，用于显示错误信息或成功信息，并在此页面上唤起客户端 App，传递 Token 值
    /// </summary>
    /// <returns></returns>
    internal static async Task<IResult> ExternalLoginDetectionCoreAsync(
        HttpContext context,
        string? error = null,
        string? token = null,
        string? port = null,
        bool useUrlSchemeLoginToken = false,
        ExternalLoginChannel channel = DefaultExternalLoginChannel)
    {
        var repo = context.RequestServices.GetRequiredService<IKeyValuePairRepository>();
        var layout = await repo.GetLayoutModelAsync(context.RequestAborted);
        var channelInt32 = unchecked((int)channel);
        var pattern = RoutePattern;
        ArgumentNullException.ThrowIfNull(pattern);
        var loginUrl = $"/{pattern.AsSpan().TrimStart('/').TrimEnd('/')}/step2/{channelInt32}";
        var isBind = context.Items[KeyIsBind] is bool isBindB && isBindB;
        LoginDetectionModel m = new()
        {
            Layout = layout,
            Token = token,
            Port = port,
            Error = error,
            UseUrlSchemeLoginToken = useUrlSchemeLoginToken,
            Channel = channel.ToString2(),
            ChannelInt32 = channelInt32,
            LoginUrl = loginUrl,
            IsBind = isBind,
        };
        return m.ToResult();
    }

    /// <summary>
    /// 根据 Steam 渠道的 <see cref="ClaimTypes.NameIdentifier"/> 获取 64 位 Id
    /// <para>示例: https://steamcommunity.com/openid/id/{val}</para>
    /// </summary>
    /// <param name="steamNameIdentifier"></param>
    /// <returns></returns>
    internal static long? GetSteamAccountId64(ReadOnlySpan<char> steamNameIdentifier)
    {
        steamNameIdentifier = steamNameIdentifier.TrimEnd('/');

        var index = steamNameIdentifier.LastIndexOf('/');
        if (index > 0)
        {
            var numberString = steamNameIdentifier[index..].Trim('/');
            if (long.TryParse(numberString, out var val))
            {
                return val;
            }
        }

        return default;
    }

    internal record struct DeviceId(Guid DeviceIdG, string? DeviceIdR, string? DeviceIdN) : IDeviceId;

    internal static readonly RecyclableMemoryStreamManager m = new();

    internal static async Task<RecyclableMemoryStream> SerializeEncryptAsync<T>(T obj, Aes aes, CancellationToken cancellationToken = default)
    {
        using var serializeStream = m.GetStream();
        await MemoryPackSerializer.SerializeAsync(serializeStream, obj, cancellationToken: cancellationToken);
        serializeStream.Position = 0;

        var encryptStream = m.GetStream();
        using CryptoStream cryptoStream = new(encryptStream, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true);
        await serializeStream.CopyToAsync(cryptoStream, cancellationToken);
        await cryptoStream.FlushFinalBlockAsync(cancellationToken);
        encryptStream.Position = 0;
        return encryptStream;
    }

    internal static async Task<string> SerializeEncryptToJsonAsync(ApiRsp<LoginOrRegisterResponse?> obj, Aes aes, CancellationToken cancellationToken = default)
    {
        using var serializeStream = m.GetStream();
        await JsonSerializer.SerializeAsync(serializeStream, obj,
            IdentityMinimalApisJsonSerializerContext.Default.ApiRspLoginOrRegisterResponse, cancellationToken: cancellationToken);
        serializeStream.Position = 0;

        using var encryptStream = m.GetStream();
        using CryptoStream cryptoStream = new(encryptStream, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true);
        await serializeStream.CopyToAsync(cryptoStream, cancellationToken);
        await cryptoStream.FlushFinalBlockAsync(cancellationToken);
        encryptStream.Position = 0;

        var span = encryptStream.GetBuffer().AsSpan()[..unchecked((int)encryptStream.Length)];
        var r = Base64Url.EncodeToString(span);
        return r;
    }
}

/// <summary>
/// 修复 Session 保存后读取无值的问题
/// </summary>
file static class FixSession
{
    /// <summary>
    /// 使用追加 Cookie 方式修复 Session 无法读取的问题
    /// </summary>
    static readonly bool AppendCookieFixSession = false;

    /// <summary>
    /// 从 Session 或 Cookie 中获取字符串值
    /// </summary>
    internal static string? GetSessionString(HttpContext context, string key)
    {
        string? value = null;
        if (!string.IsNullOrEmpty(context.Session.Id))
        {
            value = context.Session.GetString(key);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
        }
        if (AppendCookieFixSession)
        {
            context.Request.Cookies.TryGetValue($"7DCE917D.{key}", out value);
        }
        return value;
    }

    /// <summary>
    /// 从 Session 或 Cookie 中获取 <see cref="int"/> 值
    /// </summary>
    /// <param name="context"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    internal static int? GetSessionInt32(HttpContext context, string key)
    {
        int? value = null;
        if (!string.IsNullOrEmpty(context.Session.Id))
        {
            value = context.Session.GetInt32(key);
            if (value.HasValue)
            {
                return value;
            }
        }
        if (AppendCookieFixSession)
        {
            if (context.Request.Cookies.TryGetValue($"7DCE917D.{key}", out var valueS))
            {
                if (int.TryParse(valueS, out var valueI))
                {
                    return valueI;
                }
            }
        }
        return value;
    }

    internal static CookieOptions CreateCookieOptions(HttpContext context)
    {
        var options = context.RequestServices.GetRequiredService<IOptions<SessionOptions>>().Value;
        CookieOptions cookieOptions = new()
        {
            HttpOnly = options.Cookie.HttpOnly,
            IsEssential = options.Cookie.IsEssential,
            Expires = DateTimeOffset.UtcNow.Add(options.IdleTimeout),
        };
        return cookieOptions;
    }

    internal static CookieOptions GetCookieOptions(HttpContext context)
    {
        if (context.Items.TryGetValue(nameof(CookieOptions), out var options) && options is CookieOptions cookieOptions)
        {
        }
        else
        {
            cookieOptions = CreateCookieOptions(context);
            context.Items[nameof(CookieOptions)] = cookieOptions;
        }
        return cookieOptions;
    }

    /// <summary>
    /// 将指定的 Query 参数保存到 Session 中
    /// </summary>
    /// <param name="context"></param>
    internal static void SetQueryToSession(HttpContext context)
    {
        CookieOptions cookieOptions = AppendCookieFixSession ? GetCookieOptions(context) : null!;

        foreach (var item in QueryKeys)
        {
            var value = context.Request.Query[item];
            if (!StringValues.IsNullOrEmpty(value))
            {
                context.Session.SetString(item, value!);
                if (AppendCookieFixSession)
                {
                    context.Response.Cookies.Append($"7DCE917D.{item}", value!, cookieOptions);
                }
            }
        }
    }

    internal static void SetSessionInt32(HttpContext context, string key, int value)
    {
        CookieOptions cookieOptions = AppendCookieFixSession ? GetCookieOptions(context) : null!;
        context.Session.SetInt32(key, value);
        if (AppendCookieFixSession)
        {
            context.Response.Cookies.Append($"7DCE917D.{key}", value.ToString(), cookieOptions);
        }
    }
}