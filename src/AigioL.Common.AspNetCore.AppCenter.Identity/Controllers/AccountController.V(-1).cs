using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

static partial class AccountController
{
    [Obsolete("use MapIdentityAccountV2")]
    public static void MapIdentityAccount(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "api/Account")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous()
            .WithRequiredSecurityKey();

        routeGroup.MapPost("LoginOrRegister", [Obsolete] async (HttpContext context,
            [FromBody] LoginOrRegisterRequestV_1 request) =>
        {
            var r = await LoginOrRegister(context,
                request.PhoneNumber,
                default,
                request.SmsCode,
                request.Channel,
                default,
                static (userManager, user, isLoginOrRegister) => userManager.LoginSharedV0Async(user, isLoginOrRegister, default));
            return r;
        }).WithDescription("【废弃】登录或注册账号 V(-1)");
        routeGroup.MapGet("RefreshToken/{refresh_token}", [Obsolete] async (HttpContext context,
            [FromRoute] string refresh_token) =>
        {
            const string? deviceId = null;
            var r = await RefreshTokenAsync(context, refresh_token, deviceId);
            return r;
        }).WithDescription("【废弃】刷新 JWT V(-1)");
    }
}