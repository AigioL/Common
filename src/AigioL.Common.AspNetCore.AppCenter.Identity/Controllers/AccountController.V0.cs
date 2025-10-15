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
    [Obsolete("use MapIdentityAccountV2")]
    public static void MapIdentityAccountV0(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/account")
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
        }).WithDescription("【废弃】登录或注册账号 V0");
        routeGroup.MapPost("refreshtoken", [Obsolete] async (HttpContext context,
            [FromBody] RefreshTokenRequestV0 request) =>
        {
            var deviceId = request.GetDeviceId();
            var r = await RefreshTokenAsync(context, request.RefreshToken, deviceId);
            return r;
        }).WithDescription("【废弃】刷新 JWT V0");
        routeGroup.MapPost("validateRegisterEmail", [Obsolete] async (HttpContext context,
            [FromBody] ValidateRegisterEmailRequestV0 request) =>
        {
            var r = await ValidateRegisterEmail(context, request.Email);
            return r;
        }).WithDescription("【废弃】验证注册邮箱账号 V0");
        routeGroup.MapPost("resetPassword", [Obsolete] async (HttpContext context,
            [FromBody] ResetPasswordRequestV0 request) =>
        {
            var authMessageRecordRepo = context.RequestServices.GetRequiredService<IAuthMessageRecordRepository>();
            var userManager = context.RequestServices.GetRequiredService<IJsonWebTokenUserManager>();
            var smsSender = context.RequestServices.GetRequiredService<ISmsSender>();
            var r = await ResetPassword(context, authMessageRecordRepo, userManager,
                smsSender, request.Type, request.PhoneNumber,
                null, request.Email, request.OTPCode,
                request.Password, request.Password2);
            return r;
        }).WithDescription("【废弃】重置密码 V0");
        routeGroup.MapPost("registerByEmail", [Obsolete] async (HttpContext context,
            [FromBody] ResetPasswordRequestV0 request) =>
        {
            var authMessageRecordRepo = context.RequestServices.GetRequiredService<IAuthMessageRecordRepository>();
            var userManager = context.RequestServices.GetRequiredService<IJsonWebTokenUserManager>();
            var smsSender = context.RequestServices.GetRequiredService<ISmsSender>();
            var r = await ResetPassword(context, authMessageRecordRepo, userManager,
                smsSender, request.Type, request.PhoneNumber,
                null, request.Email, request.OTPCode,
                request.Password, request.Password2);
            return r;
        }).WithDescription("【废弃】邮箱注册账号 V0");
        routeGroup.MapPost("loginByPassword", [Obsolete] async (HttpContext context,
            [FromBody] ResetPasswordRequestV0 request) =>
        {
            var authMessageRecordRepo = context.RequestServices.GetRequiredService<IAuthMessageRecordRepository>();
            var userManager = context.RequestServices.GetRequiredService<IJsonWebTokenUserManager>();
            var smsSender = context.RequestServices.GetRequiredService<ISmsSender>();
            var r = await ResetPassword(context, authMessageRecordRepo, userManager,
                smsSender, request.Type, request.PhoneNumber,
                null, request.Email, request.OTPCode,
                request.Password, request.Password2);
            return r;
        }).WithDescription("【废弃】密码登录账号 V0");
    }
}