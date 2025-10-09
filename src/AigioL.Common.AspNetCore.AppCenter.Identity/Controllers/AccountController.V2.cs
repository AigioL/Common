using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

static partial class AccountController
{
    public static void MapIdentityAccountV2(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v2/account")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous()
            .WithRequiredSecurityKey();

        routeGroup.MapPost("loginorregister", async (HttpContext context,
            [FromBody] LoginOrRegisterRequestV2 request) =>
        {
            var deviceId = request.GetDeviceId();
            var r = await LoginOrRegister(context,
                request.PhoneNumber,
                request.PhoneNumberRegionCode,
                request.SmsCode,
                request.Channel,
                deviceId,
                (userManager, user, isLoginOrRegister) => userManager.LoginSharedV0Async(user, isLoginOrRegister, deviceId));
            return r;
        }).WithDescription("登录或注册账号 V2");
        routeGroup.MapPost("refreshtoken", async (HttpContext context,
            [FromBody] RefreshTokenRequestV1 request) =>
        {
            var deviceId = request.GetDeviceId();
            var r = await RefreshTokenAsync(context, request.RefreshToken, deviceId);
            return r;
        }).WithDescription("刷新 JWT V2");
#if DEBUG
        routeGroup.MapGet("test/ex", async (HttpContext context) =>
        {
            await Task.Delay(10, context.RequestAborted);
            throw new ApplicationException("测试 WithRequiredSecurityKey 的异常页面");
        }).WithIdentityUIView();
#endif

        // serversideproxytoken
        // refreshtoken
    }
}