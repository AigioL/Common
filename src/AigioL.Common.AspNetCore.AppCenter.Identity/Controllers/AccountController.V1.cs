using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

static partial class AccountController
{
    [Obsolete("use MapIdentityAccountV2")]
    public static void MapIdentityAccountV1(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v1/account")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous()
            .WithRequiredSecurityKey();

        routeGroup.MapPost("loginorregister", [Obsolete] async (HttpContext context,
            [FromBody] LoginOrRegisterRequestV0 request) =>
        {
            var deviceId = request.GetDeviceId();
            var r = await LoginOrRegister(context,
                request.PhoneNumber,
                default,
                request.SmsCode,
                request.Channel,
                deviceId,
                (userManager, user, isLoginOrRegister) => userManager.LoginSharedV0Async(user, isLoginOrRegister, deviceId));
            return r;
        }).WithDescription("【废弃】登录或注册账号 V1");
        routeGroup.MapPost("refreshtoken", [Obsolete] async (HttpContext context,
            [FromBody] RefreshTokenRequestV1 request) =>
        {
            var deviceId = request.GetDeviceId();
            var r = await RefreshTokenAsync(context, request.RefreshToken, deviceId);
            return r;
        }).WithDescription("【废弃】刷新 JWT V1");
    }
}