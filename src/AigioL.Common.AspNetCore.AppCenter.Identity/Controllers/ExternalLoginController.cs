using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Repositories.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using static AigioL.Common.AspNetCore.AppCenter.Identity.Controllers.D3b96193;
using static AigioL.Common.AspNetCore.AppCenter.Identity.Controllers.ErrorController;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

/// <summary>
/// 第三方外部登录终结点
/// </summary>
public static partial class ExternalLoginController
{
    public static void MapIdentityExternalLogin(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v1/externallogin")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapGet("start/{channel?}", async (HttpContext context,
            [FromRoute] ExternalLoginChannel channel = ExternalLoginChannel.Steam) =>
        {
            var r = await ExternalLogin(context, channel);
            return r;
        }).WithIdentityUIView();
        routeGroup.MapGet("{channel?}", async (HttpContext context,
            [FromRoute] ExternalLoginChannel channel = ExternalLoginChannel.Steam) =>
        {
            var r = await ExternalLoginDetectionAsync(context, channel);
            return r;
        }).WithIdentityUIView();
        routeGroup.MapGet("callback", async (HttpContext context) =>
        {
            var r = await Callback(context);
            return r;
        }).WithIdentityUIView();

#if DEBUG
        routeGroup.MapGet("test/ex", async (HttpContext context) =>
        {
            await Task.Delay(10, context.RequestAborted);
            throw new ApplicationException("测试 WithIdentityUIView 的异常页面");
        }).WithIdentityUIView();
#endif
    }

    /// <summary>
    /// 第三方外部登录接口（开始首步骤）
    /// </summary>
    /// <returns></returns>
    static async Task<IResult> ExternalLogin(
        HttpContext context,
        ExternalLoginChannel channel)
    {
        ResponseCacheNone(context);

        if (!Enum.IsDefined(channel))
        {
            return Results.NotFound();
        }

        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    /// <summary>
    /// 第三方外部登录接口（浏览器兼容性检测步骤）
    /// </summary>
    /// <returns></returns>
    static Task<IResult> ExternalLoginDetectionAsync(
        HttpContext context,
        ExternalLoginChannel channel)
    {
        ResponseCacheNone(context);
        context.Session.Clear();
        SetSessions(context);
        return ExternalLoginDetectionCoreAsync(context, channel: channel);
    }

    /// <summary>
    /// 第三方外部登录接口（回调接口）
    /// </summary>
    /// <returns></returns>
    static async Task<IResult> Callback(
        HttpContext context)
    {
        ResponseCacheNone(context);

        await Task.CompletedTask;
        throw new NotImplementedException();
    }
}

file static class D3b96193
{
    static readonly string[] QueryKeys = [
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

    /// <summary>
    /// 将指定的 Query 参数保存到 Session 中
    /// </summary>
    /// <param name="context"></param>
    internal static void SetSessions(HttpContext context)
    {
        foreach (var item in QueryKeys)
        {
            var value = context.Request.Query[item];
            if (!StringValues.IsNullOrEmpty(value))
            {
                context.Session.SetString(item, value!);
            }
        }
    }

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
        ExternalLoginChannel channel = ExternalLoginChannel.Steam)
    {
        var repo = context.RequestServices.GetRequiredService<IKeyValuePairRepository>();
        var layout = await repo.GetLayoutModelAsync(context.RequestAborted);
        LoginDetectionModel m = new()
        {
            Layout = layout,
            Token = token,
            Port = port,
            Error = error,
            UseUrlSchemeLoginToken = useUrlSchemeLoginToken,
            IsV1 = true,
            Channel = channel switch
            {
                ExternalLoginChannel.Xbox => nameof(ExternalLoginChannel.Microsoft),
                _ => channel.ToString(),
            },
            ChannelInt32 = unchecked((int)channel),
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
            var numberString = steamNameIdentifier[index..];
            if (long.TryParse(numberString, out var val))
            {
                return val;
            }
        }

        return default;
    }
}