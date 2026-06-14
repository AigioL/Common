using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Helpers.Sms;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

/// <summary>
/// 短信、邮箱验证码终结点
/// </summary>
public static partial class VerificationCodesController
{
    public static void MapIdentityVerificationCodesV5<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v5/vcodes")
        where TAppSettings : class, IDisableSms
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapPost("sms", async (HttpContext context,
            [FromBody] SendSmsRequest request) =>
        {
            var r = await SmsHelper.SendSms<TAppSettings, User, IUserManager2>(context, request.PhoneNumber, request.PhoneNumberRegionCode, request.Type);
            return r;
        }).WithDescription("发送短信验证码")
        .WithRequiredSecurityKey();
        routeGroup.MapPost("email", async (HttpContext context,
            [FromBody] SendEmailCodeRequest request) =>
        {
            var r = await SendEmailOtp(context, request.Email, request.Type);
            return r;
        }).WithDescription("发送邮件验证码")
        .WithRequiredSecurityKey();
        //#if DEBUG
        routeGroup.MapPost("sms-test", async (HttpContext context) =>
        {
            ApiRsp r = "测试加密接口";
            return r;
        }).WithDescription("测试加密接口")
        .WithRequiredSecurityKey();
        //#endif
    }

    /// <summary>
    /// 发送邮件验证码
    /// </summary>
    internal static async Task<ApiRsp> SendEmailOtp(
        HttpContext context,
        string email,
        SmsCodeType type)
    {
        await Task.CompletedTask;
        return "TODO: 待完成";
        //SendEmailOtpCoreAsync(this, logger, connection, authMessageRecordRepo, request);
    }
}
