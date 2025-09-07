using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

public static partial class VerificationCodesController
{
    public static void MapIdentityVerificationCodes(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v1/vcodes")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous()
            .WithRequiredSecurityKey();

        //routeGroup.MapPost("loginorregister", async (HttpContext context,
        //    [FromBody] LoginOrRegisterRequest request) =>
        //{
        //    var r = await LoginOrRegister(context, request);
        //    return r;
        //}).WithDescription("登录或注册账号");
    }

    ///// <summary>
    ///// 发送短信验证码
    ///// 需要传入接收人手机号码以及用途(类型)
    ///// 根据不同的用途过滤手机号码是否符号发送短信条件
    ///// 过滤不符合条件的不予发送短信
    ///// </summary>
    //[HttpPost("sms")]
    //public Task<ApiRspImpl> SendSms([FromBody] SendSmsRequest request)
    //    => SendSmsCoreAsync(this, logger, smsSender, authMessageRecordRepo,
    //        userManager, appSettings, request);

    ///// <summary>
    ///// 发送邮件验证码
    ///// </summary>
    //[HttpPost("email")]
    //public Task<ApiRspImpl> SendEmailOtp([FromBody] SendEmailCodeRequest request)
    //    => SendEmailOtpCoreAsync(this, logger, connection, authMessageRecordRepo, request);
}
