using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using static AigioL.Common.AspNetCore.AppCenter.Identity.Controllers.VerificationCodesController;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

[Obsolete("use VerificationCodesController")]
public static partial class AuthMessageController
{
    [Obsolete("use MapIdentityVerificationCodesV1")]
    public static void MapIdentityAuthMessage(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "api/AuthMessage")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous()
            .WithRequiredSecurityKey();

        routeGroup.MapPost("SendSms", [Obsolete] async (HttpContext context,
            [FromBody] SendSmsRequestV0 request) =>
        {
            var r = await SendSms(context, request.PhoneNumber, null, request.Type);
            return r;
        }).WithDescription("【Obsolete】发送短信验证码 V(-1)");
    }
}
