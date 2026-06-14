using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Helpers.Sms;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.Primitives.Columns;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace AigioL.Common.AspNetCore.PartnerCenter.Controllers.Infrastructure;

static partial class VerificationCodesController
{
    internal static void MapIdentityVerificationCodesV5_PC<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        this IEndpointRouteBuilder b)
        where TAppSettings : class, IDisableSms
    {
        // 👇 与旧 KolLoginController 保持路由兼容
        b.MapPost("sms/send", async (HttpContext context,
             [FromBody] string[] args) =>
        {
            var r = await SendSmsAsync<TAppSettings>(context, args);
            return r.SetHttpContext(context);
        }).WithDescription("合作伙伴后台用户发送短信验证码")
        .AllowAnonymous();
    }

    static async Task<BMApiRsp> SendSmsAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        HttpContext context,
        string[] args)
        where TAppSettings : class, IDisableSms
    {
        if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0]))
        {
            return "手机号不能为空";
        }

        var appSettings = context.RequestServices.GetRequiredService<IOptions<BMAppSettings>>().Value;

        var rsaPrivateKey = appSettings.AdminRSAPrivateKey;
        ArgumentNullException.ThrowIfNull(rsaPrivateKey);
        var rsaParameters = RSAUtils.ReadParameters(rsaPrivateKey);
        using var rsa = RSA.Create(rsaParameters);

        var phoneNumber = rsa.BMDecrypt(args[0]);
        var phoneNumberRegionCode = args.Length > 1 ? rsa.BMDecrypt(args[1]) : IPhoneNumber.DefaultPhoneNumberRegionCode;
        var type = SmsCodeType.Login;

        var r = await SmsHelper.SendSms<TAppSettings, PCUser, IIdentityUserManager<PCUser>>(context, phoneNumber, phoneNumberRegionCode, type, setUserId: SetUserId);
        return r;
    }

    static void SetUserId(AuthMessageRecord record, Guid userId)
    {
        record.PCUserId = userId;
    }
}
