using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

static partial class VerificationCodesController
{
    [Obsolete("use MapIdentityVerificationCodesV1")]
    public static void MapIdentityVerificationCodesV0(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/vcodes")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous()
            .WithRequiredSecurityKey();

        routeGroup.MapPost("sms", [Obsolete] async (HttpContext context,
            [FromBody] SendSmsRequestV0 request) =>
        {
            var r = await SendSms(context, request.PhoneNumber, null, request.Type);
            return r;
        }).WithDescription("【Obsolete】发送短信验证码 V0");
        routeGroup.MapPost("email", [Obsolete] async (HttpContext context,
            [FromBody] SendEmailCodeRequestV0 request) =>
        {
            var r = await SendEmailOtp(context, request.Email, request.Type);
            return r;
        }).WithDescription("【Obsolete】发送邮件验证码 V0");
    }
}
