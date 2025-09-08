using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Columns;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

public static partial class VerificationCodesController
{
    public static void MapIdentityVerificationCodesV0(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/vcodes")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous()
            .WithRequiredSecurityKey();

        routeGroup.MapPost("sms", [Obsolete] async (HttpContext context,
            [FromBody] SendSmsRequest request) =>
        {
            var r = await SendSms(context, request.PhoneNumber, null, request.Type);
            return r;
        }).WithDescription("【Obsolete】发送短信验证码 V0");
        routeGroup.MapPost("email", [Obsolete] async (HttpContext context,
            [FromBody] SendEmailCodeRequest request) =>
        {
            var r = await SendEmailOtp(context, request.Email, request.Type);
            return r;
        }).WithDescription("【Obsolete】发送邮件验证码 V0");
    }

    public static void MapIdentityVerificationCodesV1(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v1/vcodes")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous()
            .WithRequiredSecurityKey();

        routeGroup.MapPost("sms", async (HttpContext context,
            [FromBody] SendSmsRequestV1 request) =>
        {
            var r = await SendSms(context, request.PhoneNumber, request.PhoneNumberRegionCode, request.Type);
            return r;
        }).WithDescription("发送短信验证码 V1");
        routeGroup.MapPost("email", async (HttpContext context,
            [FromBody] SendEmailCodeRequestV1 request) =>
        {
            var r = await SendEmailOtp(context, request.Email, request.Type);
            return r;
        }).WithDescription("发送邮件验证码 V1");
    }

    /// <summary>
    /// 发送短信验证码
    /// 需要传入接收人手机号码以及用途(类型)
    /// 根据不同的用途过滤手机号码是否符号发送短信条件
    /// 过滤不符合条件的不予发送短信
    /// </summary>
    internal static async Task<ApiRsp> SendSms(
        HttpContext context,
        string? phoneNumber,
        string? phoneNumberRegionCode,
        SmsCodeType type)
    {
        if (string.IsNullOrWhiteSpace(phoneNumberRegionCode))
        {
            phoneNumberRegionCode = IPhoneNumber.DefaultPhoneNumberRegionCode;
        }

        await Task.CompletedTask;
        throw new NotImplementedException();
        //SendSmsCoreAsync(this, logger, smsSender, authMessageRecordRepo,
        //    userManager, appSettings, request);
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
        throw new NotImplementedException();
        //SendEmailOtpCoreAsync(this, logger, connection, authMessageRecordRepo, request);
    }
}
