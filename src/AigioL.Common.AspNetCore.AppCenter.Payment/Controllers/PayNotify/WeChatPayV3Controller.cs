using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using V2 = global::Essensoft.Paylink.WeChatPay.V2;
using V2Notify = global::Essensoft.Paylink.WeChatPay.V2.Notify;
using V3 = global::Essensoft.Paylink.WeChatPay.V3;
using V3Notify = global::Essensoft.Paylink.WeChatPay.V3.Notify;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Controllers.PayNotify;

public static partial class WeChatPayV3Controller
{
    public static void MapPaymentNotifyWeChatPayV3(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "payment/notify/wechatpay")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapPost("", async (HttpContext context) =>
        {
            var r = await UnifiedOrder(context);
            return r;
        }).WithDescription("微信支付统一下单支付结果通知");
        routeGroup.MapPost("refund", async (HttpContext context) =>
        {
            var r = await Refund(context);
            return r;
        }).WithDescription("微信支付退款结果通知");
        routeGroup.MapPost("agreement", async (HttpContext context) =>
        {
            var r = await HandleAgreementNotice(context);
            return r;
        }).WithDescription("微信支付处理签约/解约结果通知");
        routeGroup.MapPost("v2/agreement", async (HttpContext context) =>
        {
            var r = await HandleAgreementNotice(context);
            return r;
        }).WithDescription("微信支付处理签约/解约结果通知 V2");
        routeGroup.MapPost("transfer", async (HttpContext context) =>
        {
            var r = await TransferNotify(context);
            return r;
        }).WithDescription("微信商家转账回调通知");
    }

    /// <summary>
    /// 微信支付统一下单支付结果通知
    /// <para>https://{host}/payment/notify/wechatpay</para>
    /// </summary>
    static Task<IResult> UnifiedOrder(
        HttpContext context)
    {
        return GetResultAsync(context, "支付结果通知", v2, v3);

        static async Task<IResult> v2(HttpContext context)
        {
            var clientV2 = context.RequestServices.GetRequiredService<V2.IWeChatPayNotifyClient>();
            var paymentOptions = context.RequestServices.GetRequiredService<IOptions<WeChatPayExOptions>>().Value;
            var paymentService = context.RequestServices.GetRequiredService<IPaymentService>();

            var notify = await clientV2.ExecuteAsync<V2Notify.WeChatPayUnifiedOrderNotify>(context.Request, paymentOptions);
            if (notify.ReturnCode == V2.WeChatPayCode.Success &&
                notify.ResultCode == V2.WeChatPayCode.Success)
            {
                var amountReceived = notify.TotalFee / 100M;
                var paymentTime = DateTimeOffset.ParseExact(notify.TimeEnd, "yyyyMMddHHmmss", CultureInfo.InvariantCulture.DateTimeFormat);
                await paymentService.NotifyOrderComplete(
                    notify.OutTradeNo,
                    notify.TransactionId,
                    PaymentType.WeChatPay,
                    amountReceived,
                    paymentTime);

                return WeChatPayNotifyResults.V2.Success;
            }
            return WeChatPayNotifyResults.V2.Failure;
        }

        static async Task<IResult> v3(HttpContext context)
        {
            var clientV3 = context.RequestServices.GetRequiredService<V3.IWeChatPayNotifyClient>();
            var paymentOptions = context.RequestServices.GetRequiredService<IOptions<WeChatPayExOptions>>().Value;
            var paymentService = context.RequestServices.GetRequiredService<IPaymentService>();

            var notify = await clientV3.ExecuteAsync<V3Notify.WeChatPayTransactionsNotify>(context.Request, paymentOptions);
            if (notify.TradeState == V3.WeChatPayTradeState.Success)
            {
                var amountReceived = (notify.Amount.Total ?? 0) / 100M;
                var paymentTime = DateTimeOffset.Parse(notify.SuccessTime);
                await paymentService.NotifyOrderComplete(
                    notify.OutTradeNo,
                    notify.TransactionId,
                    PaymentType.WeChatPay,
                    amountReceived,
                    paymentTime);

                return WeChatPayNotifyResults.V3.Success;
            }
            return WeChatPayNotifyResults.V3.Failure;
        }
    }

    /// <summary>
    /// 微信支付退款结果通知
    /// <para>https://{host}/payment/notify/wechatpay/refund</para>
    /// </summary>
    static Task<IResult> Refund(
        HttpContext context)
    {
        return GetResultAsync(context, "退款结果通知", v2, v3);

        static async Task<IResult> v2(HttpContext context)
        {
            var clientV2 = context.RequestServices.GetRequiredService<V2.IWeChatPayNotifyClient>();
            var paymentOptions = context.RequestServices.GetRequiredService<IOptions<WeChatPayExOptions>>().Value;
            var paymentService = context.RequestServices.GetRequiredService<IPaymentService>();
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(WeChatPayV3Controller));
            var paymentMessageQueue = context.RequestServices.GetRequiredService<IPaymentMessageQueueService>();
            var paymentRepo = context.RequestServices.GetRequiredService<IPaymentRepository>();

            var notify = await clientV2.ExecuteAsync<V2Notify.WeChatPayRefundNotify>(context.Request, paymentOptions);
            if (notify.ReturnCode == V2.WeChatPayCode.Success &&
                notify.RefundStatus == V2.WeChatPayCode.Success)
            {
                var refundInfo = new OrderRefundSuccessInfo(
                    notify.OutTradeNo,
                    notify.OutRefundNo,
                    PaymentType.WeChatPay);

                await paymentRepo.CompleteRefundForOrderAsync(refundInfo);    // 完成订单退款
                await paymentMessageQueue.PushRefundSuccess(refundInfo); // 推送退款完成的消息

                logger.LogInformation("微信退款成功：{RefundInfo}", refundInfo);

                return WeChatPayNotifyResults.V2.Success;
            }
            return WeChatPayNotifyResults.V2.Failure;
        }

        static async Task<IResult> v3(HttpContext context)
        {
            var clientV3 = context.RequestServices.GetRequiredService<V3.IWeChatPayNotifyClient>();
            var paymentOptions = context.RequestServices.GetRequiredService<IOptions<WeChatPayExOptions>>().Value;
            var paymentService = context.RequestServices.GetRequiredService<IPaymentService>();
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(WeChatPayV3Controller));
            var paymentMessageQueue = context.RequestServices.GetRequiredService<IPaymentMessageQueueService>();
            var paymentRepo = context.RequestServices.GetRequiredService<IPaymentRepository>();

            var notify = await clientV3.ExecuteAsync<V3Notify.WeChatPayRefundDomesticRefundsNotify>(context.Request, paymentOptions);
            if (notify.RefundStatus == V3.WeChatPayRefundStatus.Success)
            {
                var refundInfo = new OrderRefundSuccessInfo(
                    notify.OutTradeNo,
                    notify.OutRefundNo,
                    PaymentType.WeChatPay);

                await paymentRepo.CompleteRefundForOrderAsync(refundInfo);    // 完成订单退款
                await paymentMessageQueue.PushRefundSuccess(refundInfo); // 推送退款完成的消息

                logger.LogInformation("微信退款成功：{RefundInfo}", refundInfo);

                return WeChatPayNotifyResults.V3.Success;
            }
            return WeChatPayNotifyResults.V3.Failure;
        }
    }

    /// <summary>
    /// 微信支付处理签约/解约结果通知
    /// <para>https://{host}/payment/notify/wechatpay/agreement</para>
    /// </summary>
    static Task<IResult> HandleAgreementNotice(
        HttpContext context)
    {
        return GetResultAsync(context, "协议签约、解约结果通知", v2, v3);

        static async Task<IResult> v2(HttpContext context)
        {
            var clientV2 = context.RequestServices.GetRequiredService<V2.IWeChatPayNotifyClient>();
            var paymentOptions = context.RequestServices.GetRequiredService<IOptions<WeChatPayExOptions>>().Value;
            var paymentService = context.RequestServices.GetRequiredService<IPaymentService>();
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(WeChatPayV3Controller));
            var paymentMessageQueue = context.RequestServices.GetRequiredService<IPaymentMessageQueueService>();
            var agreementRepo = context.RequestServices.GetRequiredService<IMerchantDeductionAgreementRepository>();

            var notify = await clientV2.ExecuteAsync<V2Notify.WeChatPayPaPayPartnerEntrustNotify>(context.Request, paymentOptions);
            if (string.Equals("ADD", notify.ChangeType, StringComparison.InvariantCultureIgnoreCase)) // 签约
            {
                await agreementRepo.CompleteAgreementSign(new MerchantDeductionAgreement()
                {
                    ExtAgreementNo = notify.ContractId,
                    AgreementNo = notify.ContractCode,
                    SigningTime = DateTimeOffset.Parse(notify.OperateTime),
                    ValidTime = DateTimeOffset.Parse(notify.OperateTime),
                    InvalidTime = DateTimeOffset.Parse(notify.ContractExpiredTime),
                    UserOpenId = notify.OpenId,
                    PeriodType = "",
                });
                await paymentMessageQueue.PushSignAgreementSuccess(notify.ContractCode);
                return WeChatPayNotifyResults.V2.Success;
            }
            else if (string.Equals("DELETE", notify.ChangeType, StringComparison.InvariantCultureIgnoreCase)) // 解约
            {
                await agreementRepo.CompleteAgreementUnSign(notify.ContractCode, DateTimeOffset.Parse(notify.OperateTime));
                await paymentMessageQueue.PushUnSignAgreementSuccess(notify.ContractCode);
                return WeChatPayNotifyResults.V2.Success;
            }
            else
            {
                logger.LogError("签约/解约结果通知错误，未知的协议变动类型。{ChangeType}", notify.ChangeType);
            }
            return WeChatPayNotifyResults.V2.Failure;
        }

        static Task<IResult> v3(HttpContext context)
        {
            // UNDONE: 微信还没有 V3 版本的签约/解约通知接口
            return Task.FromResult(WeChatPayNotifyResults.V3.Failure);
        }
    }

    /// <summary>
    /// 微信商家转账回调通知
    /// <para>https://{host}/payment/notify/wechatpay/transfer</para>
    /// </summary>
    static async Task<IResult> TransferNotify(HttpContext context)
    {
        try
        {
            var clientV3 = context.RequestServices.GetRequiredService<V3.IWeChatPayNotifyClient>();
            var paymentOptions = context.RequestServices.GetRequiredService<IOptions<WeChatPayExOptions>>().Value;
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(WeChatPayV3Controller));

            var notify = await clientV3.ExecuteAsync<WeChatPayTransferBillNotify>(context.Request, paymentOptions);

            logger.LogInformation(
                "微信商家转账回调：OutBillNo={OutBillNo}, TransferBillNo={TransferBillNo}, State={State}, Amount={Amount}",
                notify.OutBillNo, notify.TransferBillNo, notify.State, notify.TransferAmount);

            // TODO: 根据 notify.OutBillNo 更新对应的 PCUserWithdrawalRecord 状态
            // - State == "SUCCESS" → 更新提现记录为 Success
            // - State == "FAIL" → 更新提现记录为 Failed，退回金额到钱包可提现金额
            // - State == "CANCELLED" → 更新提现记录为 Failed，退回金额到钱包可提现金额

            return WeChatPayNotifyResults.V3.Success;
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetService<ILoggerFactory>()?.CreateLogger(nameof(WeChatPayV3Controller));
            logger?.LogError(ex, "微信商家转账回调处理异常");
            return WeChatPayNotifyResults.V3.Failure;
        }
    }

    static async Task<IResult> GetResultAsync(
        HttpContext context,
        string notifyType,
        Func<HttpContext, Task<IResult>> v2,
        Func<HttpContext, Task<IResult>> v3)
    {
        bool isV2Request = false;
        try
        {
            var contentType = context.Request.ContentType;
            if (contentType != null && contentType.Contains("text/xml"))
            {
                isV2Request = true;
            }

            if (isV2Request)
            {
                var result = await v2(context);
                return result;
            }
            else
            {
                var result = await v3(context);
                return result;
            }
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetService<ILoggerFactory>()?.CreateLogger(nameof(WeChatPayV3Controller));
            if (logger != null)
            {
                LogError(logger, ex, notifyType);
            }

            if (isV2Request)
            {
                return WeChatPayNotifyResults.V2.Failure;
            }
            else
            {
                return WeChatPayNotifyResults.V3.Failure;
            }
        }
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message =
"""
微信支付通知回调异常
通知类型：{notifyType}
""")]
    static partial void LogError(ILogger logger, Exception exception, string notifyType);
}

/// <summary>
/// WeChatPay 通知应答
/// </summary>
file static class WeChatPayNotifyResults
{
    /// <summary>
    /// WeChatPay V2 通知应答
    /// </summary>
    public static class V2
    {
        public readonly struct SuccessResult : IResult
        {
            Task IResult.ExecuteAsync(HttpContext httpContext) => ExecuteSuccessAsync(httpContext);
        }

        public readonly struct FailureResult : IResult
        {
            Task IResult.ExecuteAsync(HttpContext httpContext) => ExecuteFailureAsync(httpContext);
        }

        public static IResult Success => default(SuccessResult);

        public static IResult Failure => default(FailureResult);

        public static async Task ExecuteSuccessAsync(HttpContext context)
        {
            context.Response.ContentType = "text/xml; charset=utf-8";
            context.Response.StatusCode = StatusCodes.Status200OK;
            var w = context.Response.BodyWriter;
            w.Write("<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[SUCCESS]]></return_msg></xml>"u8);
            await w.FlushAsync(context.RequestAborted);
        }

        public static async Task ExecuteFailureAsync(HttpContext context)
        {
            context.Response.ContentType = "text/xml; charset=utf-8";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var w = context.Response.BodyWriter;
            w.Write("<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[FAIL]]></return_msg></xml>"u8);
            await w.FlushAsync(context.RequestAborted);
        }
    }

    /// <summary>
    /// WeChatPay V3 通知应答
    /// </summary>
    public static class V3
    {
        public readonly struct SuccessResult : IResult
        {
            Task IResult.ExecuteAsync(HttpContext httpContext) => ExecuteSuccessAsync(httpContext);
        }

        public readonly struct FailureResult : IResult
        {
            Task IResult.ExecuteAsync(HttpContext httpContext) => ExecuteFailureAsync(httpContext);
        }

        public static IResult Success => default(SuccessResult);

        public static IResult Failure => default(FailureResult);

        public static async Task ExecuteSuccessAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = StatusCodes.Status200OK;
            var w = context.Response.BodyWriter;
            w.Write("{\"code\":\"SUCCESS\",\"message\":\"SUCCESS\"}"u8);
            await w.FlushAsync(context.RequestAborted);
        }

        public static async Task ExecuteFailureAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var w = context.Response.BodyWriter;
            w.Write("{\"code\":\"FAIL\",\"message\":\"FAIL\"}"u8);
            await w.FlushAsync(context.RequestAborted);
        }
    }
}