using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.SmsSender.Services;
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
        routeGroup.MapPost("validateRegisterEmail", async (HttpContext context,
            [FromBody] ValidateRegisterEmailRequestV2 request) =>
        {
            var r = await ValidateRegisterEmail(context, request.Email);
            return r;
        }).WithDescription("验证注册邮箱账号 V2");
        routeGroup.MapPost("resetPassword", async (HttpContext context,
            [FromBody] ResetPasswordRequestV2 request) =>
        {
            var authMessageRecordRepo = context.RequestServices.GetRequiredService<IAuthMessageRecordRepository>();
            var userManager = context.RequestServices.GetRequiredService<IJsonWebTokenUserManager>();
            var smsSender = context.RequestServices.GetRequiredService<ISmsSender>();
            var r = await ResetPassword(context, authMessageRecordRepo, userManager,
                smsSender, request.Type, request.PhoneNumber,
                request.PhoneNumberRegionCode, request.Email, request.OTPCode,
                request.Password, request.Password2);
            return r;
        }).WithDescription("重置密码 V2");
        routeGroup.MapPost("registerByEmail", async (HttpContext context,
            [FromBody] ResetPasswordRequestV2 request) =>
        {
        }).WithDescription("邮箱注册账号 V2");
        routeGroup.MapPost("loginByPassword", async (HttpContext context,
            [FromBody] ResetPasswordRequestV2 request) =>
        {
        }).WithDescription("密码登录账号 V2");
#if DEBUG
        routeGroup.MapGet("test/ex", async (HttpContext context) =>
        {
            await Task.Delay(10, context.RequestAborted);
            throw new ApplicationException("测试 WithRequiredSecurityKey 的异常页面");
        }).WithIdentityUIView();
#endif
    }
}