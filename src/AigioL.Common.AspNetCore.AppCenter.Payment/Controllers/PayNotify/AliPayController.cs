using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models;
using Essensoft.Paylink.Alipay;
using Essensoft.Paylink.Alipay.Notify;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Controllers.PayNotify;

public static class AliPayController
{
    public static void MapPaymentNotifyAliPay(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "payment/notify/alipay")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapPost("", async (HttpContext context) =>
        {
            var r = await UnifiedOrder(context);
            return r;
        }).WithDescription("支付宝应用网关（用于接收由支付宝服务器通知）");
    }

    /// <summary>
    /// 支付宝应用网关（用于接收由支付宝服务器通知）
    /// <para>https://{host}/payment/notify/alipay</para>
    /// </summary>
    static async Task<IResult> UnifiedOrder(
        HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(AliPayController));
        try
        {
            var notifyType = context.Request.Form["notify_type"];
            var r = (string?)notifyType switch
            {
                // 支付宝交易状态通知
                "trade_status_sync" => await HandlePaymentNotice(logger, context),
                // 商家扣款协议签约/解约结果通知
                "dut_user_sign" => await HandleAgreementNotice(logger, context, false),
                "dut_user_unsign" => await HandleAgreementNotice(logger, context, true),
                _ => throw new ArgumentOutOfRangeException(nameof(notifyType), notifyType, null),
            };
            return r;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "支付宝回调异常");
        }
        return AlipayNotifyResult.Failure;
    }

    /// <summary>
    /// 处理支付
    /// </summary>
    static async Task<IResult> HandlePaymentNotice(ILogger logger, HttpContext context)
    {
        var client = context.RequestServices.GetRequiredService<IAlipayNotifyClient>();
        var alipayOptionsFactory = context.RequestServices.GetRequiredService<IOptionsFactory<AlipayExOptions>>();
        var paymentOptions = GetPaymentOptions(context, alipayOptionsFactory);
        var paymentService = context.RequestServices.GetRequiredService<IPaymentService>();

        var notify = await client.RequestExecuteAsync<AlipayTradeAppPayNotify>(context.Request, paymentOptions);
        if (notify.TradeStatus != AlipayTradeStatus.Success)
        {
            return AlipayNotifyResult.Success;
        }
        // 退款的通知也无需处理
        if (!string.IsNullOrEmpty(notify.GmtRefund))
        {
            return AlipayNotifyResult.Success;
        }

        await paymentService.NotifyOrderComplete(
            notify.OutTradeNo,
            notify.TradeNo,
            PaymentType.Alipay,
            decimal.Parse(notify.TotalAmount),
            DateTimeOffset.Parse(notify.GmtPayment));

        return AlipayNotifyResult.Success;
    }

    /// <summary>
    /// 处理签约/解约结果通知
    /// </summary>
    static async Task<IResult> HandleAgreementNotice(ILogger logger, HttpContext context, bool isUnSign)
    {
        var status = context.Request.Form["status"];
        if (!isUnSign) // 签约
        {
            if (status == "NORMAL")
            {
                var client = context.RequestServices.GetRequiredService<IAlipayNotifyClient>();
                var alipayOptionsFactory = context.RequestServices.GetRequiredService<IOptionsFactory<AlipayExOptions>>();
                var paymentOptions = GetPaymentOptions(context, alipayOptionsFactory);
                var agreementRepo = context.RequestServices.GetRequiredService<IMerchantDeductionAgreementRepository>();
                var paymentMessageQueue = context.RequestServices.GetRequiredService<IPaymentMessageQueueService>();

                var notify = await client.RequestExecuteAsync<AlipayUserAgreementSignNotify>(context.Request, paymentOptions);

                await agreementRepo.CompleteAgreementSign(new MerchantDeductionAgreement()
                {
                    ExtAgreementNo = notify.AgreementNo,
                    AgreementNo = notify.ExternalAgreementNo,
                    SigningTime = DateTimeOffset.Parse(notify.SignTime),
                    UserOpenId = notify.AlipayOpenId,
                    UserLoginAccount = notify.AlipayLogonId,
                    ValidTime = DateTimeOffset.Parse(notify.ValidTime),
                    InvalidTime = DateTimeOffset.Parse(notify.InvalidTime),
                    PeriodType = "",
                });
                await paymentMessageQueue.PushSignAgreementSuccess(notify.ExternalAgreementNo);
                return AlipayNotifyResult.Success;
            }
        }
        else // 解约
        {
            if (status == "UNSIGN")
            {
                var client = context.RequestServices.GetRequiredService<IAlipayNotifyClient>();
                var alipayOptionsFactory = context.RequestServices.GetRequiredService<IOptionsFactory<AlipayExOptions>>();
                var paymentOptions = GetPaymentOptions(context, alipayOptionsFactory);
                var agreementRepo = context.RequestServices.GetRequiredService<IMerchantDeductionAgreementRepository>();
                var paymentMessageQueue = context.RequestServices.GetRequiredService<IPaymentMessageQueueService>();

                var notify = await client.RequestExecuteAsync<AlipayUserAgreementUnSignNotify>(context.Request, paymentOptions);

                await agreementRepo.CompleteAgreementUnSign(notify.ExternalAgreementNo, DateTimeOffset.Parse(notify.UnsignTime));
                await paymentMessageQueue.PushUnSignAgreementSuccess(notify.ExternalAgreementNo);
                return AlipayNotifyResult.Success;
            }
        }
        return AlipayNotifyResult.Failure;
    }

    static AlipayExOptions GetPaymentOptions(HttpContext context, IOptionsFactory<AlipayExOptions> alipayOptionsFactory)
    {
        context.Request.Form.TryGetValue("app_id", out var appId);

        // 默认配置
        var defaultOptions = alipayOptionsFactory.Create(Options.DefaultName);
        if (StringValues.IsNullOrEmpty(appId) || appId == defaultOptions.AppId)
        {
            return defaultOptions;
        }

        // 小程序配置
        var miniProgramOptions = alipayOptionsFactory.Create("Mini");
        if (appId == miniProgramOptions.AppId)
        {
            return miniProgramOptions;
        }

        return defaultOptions;
    }
}

file static class AlipayNotifyResult
{
    internal static readonly IResult Success = Results.Text("success", MediaTypeNames.Text.Plain, statusCode: 200);

    internal static readonly IResult Failure = Results.Text("failure", MediaTypeNames.Text.Plain, statusCode: 200);
}

file sealed class AlipayUserAgreementSignNotify : AlipayNotify
{
    [JsonPropertyName("charset")]
    public string Charset { get; set; } = string.Empty;

    [JsonPropertyName("notify_time")]
    public string NotifyTime { get; set; } = string.Empty;

    [JsonPropertyName("alipay_open_id")]
    public string AlipayOpenId { get; set; } = string.Empty;

    [JsonPropertyName("sign")]
    public string Sign { get; set; } = string.Empty;

    [JsonPropertyName("external_agreement_no")]
    public string ExternalAgreementNo { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("sign_time")]
    public string SignTime { get; set; } = string.Empty;

    [JsonPropertyName("notify_id")]
    public string NotifyId { get; set; } = string.Empty;

    [JsonPropertyName("notify_type")]
    public string NotifyType { get; set; } = string.Empty;

    [JsonPropertyName("agreement_no")]
    public string AgreementNo { get; set; } = string.Empty;

    [JsonPropertyName("invalid_time")]
    public string InvalidTime { get; set; } = string.Empty;

    [JsonPropertyName("auth_app_id")]
    public string AuthAppId { get; set; } = string.Empty;

    [JsonPropertyName("personal_product_code")]
    public string PersonalProductCode { get; set; } = string.Empty;

    [JsonPropertyName("valid_time")]
    public string ValidTime { get; set; } = string.Empty;

    [JsonPropertyName("login_token")]
    public string LoginToken { get; set; } = string.Empty;

    [JsonPropertyName("app_id")]
    public string AppId { get; set; } = string.Empty;

    [JsonPropertyName("sign_type")]
    public string SignType { get; set; } = string.Empty;

    [JsonPropertyName("sign_scene")]
    public string SignScene { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("alipay_logon_id")]
    public string AlipayLogonId { get; set; } = string.Empty;
}

file sealed class AlipayUserAgreementUnSignNotify : AlipayNotify
{
    [JsonPropertyName("charset")]
    public string Charset { get; set; } = string.Empty;

    [JsonPropertyName("notify_time")]
    public string NotifyTime { get; set; } = string.Empty;

    [JsonPropertyName("unsign_time")]
    public string UnsignTime { get; set; } = string.Empty;

    [JsonPropertyName("alipay_user_id")]
    public string AlipayUserId { get; set; } = string.Empty;

    [JsonPropertyName("sign")]
    public string Sign { get; set; } = string.Empty;

    [JsonPropertyName("external_agreement_no")]
    public string ExternalAgreementNo { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("notify_id")]
    public string NotifyId { get; set; } = string.Empty;

    [JsonPropertyName("notify_type")]
    public string NotifyType { get; set; } = string.Empty;

    [JsonPropertyName("agreement_no")]
    public string AgreementNo { get; set; } = string.Empty;

    [JsonPropertyName("auth_app_id")]
    public string AuthAppId { get; set; } = string.Empty;

    [JsonPropertyName("personal_product_code")]
    public string PersonalProductCode { get; set; } = string.Empty;

    [JsonPropertyName("app_id")]
    public string AppId { get; set; } = string.Empty;

    [JsonPropertyName("sign_type")]
    public string SignType { get; set; } = string.Empty;

    [JsonPropertyName("alipay_logon_id")]
    public string AlipayLogonId { get; set; } = string.Empty;

    [JsonPropertyName("sign_scene")]
    public string SignScene { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}