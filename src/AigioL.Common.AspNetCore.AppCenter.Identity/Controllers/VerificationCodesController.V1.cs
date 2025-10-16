using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

static partial class VerificationCodesController
{
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
}
