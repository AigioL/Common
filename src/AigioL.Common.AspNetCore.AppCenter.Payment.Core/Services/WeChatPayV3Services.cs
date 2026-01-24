using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using Essensoft.Paylink.WeChatPay;
using Essensoft.Paylink.WeChatPay.V3;
using Essensoft.Paylink.WeChatPay.V3.Domain;
using Essensoft.Paylink.WeChatPay.V3.Request;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Services;

sealed partial class WeChatPayV3Services<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
    IServiceProvider serviceProvider,
    IWeChatPayClient client,
    IOptions<TAppSettings> options,
    IOptions<WeChatPayExOptions> weChatPayOptions,
    ILogger<WeChatPayServices> logger) :
    WeChatPayV2Services<TAppSettings>(serviceProvider, options, weChatPayOptions, logger),
    IWeChatPayServices
    where TAppSettings : class, IPaySettings
{
    public async Task<PubPayState> PubPay(WeChatPayTradeType tradeType, string orderNumber, string title,
       decimal amount, string body, string ip, DateTimeOffset timeExpire, string? userOpenId = null, string? returnUrl = null)
    {
        var amountInfo = new Amount() { Total = FormatAmount(amount), Currency = "CNY" };
        var timeExpireValue = timeExpire.ToString("yyyy-MM-dd'T'HH:mm:ssK");
        var notifyUrl = PaymentOptions.NotifyUrl ?? string.Empty;

        switch (tradeType)
        {
            case WeChatPayTradeType.MWEB:
                {
                    var model = new WeChatPayTransactionsH5BodyModel
                    {
                        AppId = PaymentOptions.AppId,
                        MchId = PaymentOptions.MchId,
                        Amount = amountInfo,
                        Description = title,
                        NotifyUrl = notifyUrl,
                        OutTradeNo = orderNumber,
                        TimeExpire = timeExpireValue,
                        // ---------------------------------
                        SceneInfo = new() { PayerClientIp = ip, H5Info = new() { Type = "Wap" } },
                    };
                    var request = new WeChatPayTransactionsH5Request();
                    request.SetBodyModel(model);

                    LogApiInfo(model);
                    var response = await client.ExecuteAsync(request, PaymentOptions);
                    LogApiInfo(model.OutTradeNo, model, response, !response.IsError);

                    return response.H5Url != null
                        ? new() { IsSuccess = true, Url = response.H5Url }
                        : new() { IsSuccess = false };
                }

            case WeChatPayTradeType.NATIVE:
                {
                    var model = new WeChatPayTransactionsNativeBodyModel
                    {
                        AppId = PaymentOptions.AppId,
                        MchId = PaymentOptions.MchId,
                        Amount = amountInfo,
                        Description = title,
                        NotifyUrl = notifyUrl,
                        OutTradeNo = orderNumber,
                        TimeExpire = timeExpireValue,
                        // ---------------------------------
                    };
                    var request = new WeChatPayTransactionsNativeRequest();
                    request.SetBodyModel(model);

                    LogApiInfo(model);
                    var response = await client.ExecuteAsync(request, PaymentOptions);
                    LogApiInfo(model.OutTradeNo, model, response, !response.IsError);

                    return response.CodeUrl != null && response.CodeUrl.Contains("weixin://wxpay/bizpayurl?pr=")
                        ? new() { IsSuccess = true, Url = response.CodeUrl }
                        : new() { IsSuccess = false, Message = "找不到二维码 Url" };
                }

            case WeChatPayTradeType.JSAPI_OFFICIAL:
                {
                    var model = new WeChatPayTransactionsJsApiBodyModel
                    {
                        AppId = PaymentOptions.AppId,
                        MchId = PaymentOptions.MchId,
                        Amount = amountInfo,
                        Description = title,
                        NotifyUrl = notifyUrl,
                        OutTradeNo = orderNumber,
                        TimeExpire = timeExpireValue,
                        // ---------------------------------
                        Payer = new PayerInfo { OpenId = userOpenId },
                    };

                    var request = new WeChatPayTransactionsJsApiRequest();
                    request.SetBodyModel(model);

                    LogApiInfo(model);
                    var response = await client.ExecuteAsync(request, PaymentOptions);
                    LogApiInfo(model.OutTradeNo, model, response, !response.IsError);

                    if (string.IsNullOrEmpty(response.PrepayId))
                        return new() { IsSuccess = false, Message = "缺少预支付交易会话标识 PrepayId" };

                    var request2 = new WeChatPayJsApiSdkRequest { Package = "prepay_id=" + response.PrepayId };

                    LogApiInfo(request2.GetParameters());
                    var parameter = await client.ExecuteAsync(request2, PaymentOptions);
                    var urlValid = TryBuildPaymentPageUrl(returnUrl, parameter, out var paymentPageUrl);
                    LogApiInfo(request2.Package, request2.GetParameters(), parameter, urlValid);

                    return urlValid
                        ? new() { IsSuccess = true, Url = paymentPageUrl }
                        : new() { IsSuccess = false, Message = "获取 JsApi 参数失败" };
                }

            default:
                return new() { IsSuccess = false, Message = "不支持的交易类型" };
        }

        bool TryBuildPaymentPageUrl(string? returnUrl, WeChatPayDictionary parameter, out string paymentPageUrl)
        {
            string[] parameters = { "appId", "timeStamp", "nonceStr", "package", "signType", "paySign" };
            var qs = HttpUtility.ParseQueryString("");
            foreach (var param in parameters)
            {
                if (parameter.TryGetValue(param, out var value))
                    qs.Add(param, value);
            }
            if (!string.IsNullOrEmpty(returnUrl))
            {
                qs.Add("redirectUrl", returnUrl);
            }
            var ub = new UriBuilder(OfficialUrl) { Path = "/pay/wechatpay", Query = qs.ToString() };
            paymentPageUrl = ub.ToString();
            return true;
        }
    }

    public async Task<(bool Success, bool RefundSuccess, string Code, string ErrorDesc)> Refund(string orderNumber, string refundNumber, decimal refundAmount, decimal totalAmount)
    {
        var model = new WeChatPayRefundDomesticRefundsBodyModel()
        {
            OutTradeNo = orderNumber,
            OutRefundNo = refundNumber,
            Amount = new RefundAmount
            {
                Refund = FormatAmount(refundAmount),
                Total = FormatAmount(totalAmount),
                Currency = "CNY",
            },
            NotifyUrl = PaymentOptions.RefundNotifyUrl,
        };

        var req = new WeChatPayRefundDomesticRefundsRequest();
        req.SetBodyModel(model);

        LogApiInfo(model);
        var res = await client.ExecuteAsync(req, PaymentOptions);
        LogApiInfo(model.OutTradeNo, req, res, !res.IsError);

        if (!res.IsError)
        {
            if (res.Status == "SUCCESS")
                return (true, true, res.Code, res.Message);
            if (res.Status == "PROCESSING")
                return (true, true, res.Code, res.Message);
            //return (true, false, "");
        }
        return (false, false, res.Code, res.Message);
    }

    public async Task<bool?> OrderClose(string orderNumber)
    {
        var model = new WeChatPayTransactionsOutTradeNoCloseBodyModel { MchId = PaymentOptions.MchId };
        var request = new WeChatPayTransactionsOutTradeNoCloseRequest { OutTradeNo = orderNumber };
        request.SetBodyModel(model);

        LogApiInfo(model);
        var response = await client.ExecuteAsync(request, PaymentOptions);
        LogApiInfo(request.OutTradeNo, request, response, !response.IsError);

        if (!response.IsError)
            return true;

        logger.LogError("微信关闭订单错误 {Code} {Message}", response.Code, response.Message);
        return false;
    }

    public async Task<WechatPayTradeResult?> OrderQuery(string orderNumber)
    {
        //var model = new WeChatPayTransactionsIdQueryModel { MchId = options.MchId };
        //var request = new WeChatPayTransactionsIdRequest { TransactionId = $TransactionId };
        var model = new WeChatPayTransactionsOutTradeNoQueryModel { MchId = PaymentOptions.MchId };
        var req = new WeChatPayTransactionsOutTradeNoRequest { OutTradeNo = orderNumber };
        req.SetQueryModel(model);

        var res = await client.ExecuteAsync(req, PaymentOptions);
        if (res.IsError)
            return null;

        var tradeStatus = GetTradeStatus(res.TradeState);
        if (tradeStatus == null)
            return null;

        var tradeResult = new WechatPayTradeResult(
            res.Payer.OpenId,
            res.OutTradeNo,
            DateTimeOffset.Parse(res.SuccessTime),
            RevertAmount(res.Amount.Total ?? 0),
            res.TransactionId,
            GetTradeStatus(res.TradeState)!.Value);

        return tradeResult;
    }

    public async Task<RefundResult?> RefundQuery(string orderNumber, string refundNumber)
    {
        var req = new WeChatPayRefundDomesticRefundsOutRefundNoRequest { OutRefundNo = refundNumber };
        var res = await client.ExecuteAsync(req, PaymentOptions);
        if (res.IsError || res.Amount == null)
            return null;

        var refundResult = new RefundResult(
            res.OutRefundNo,
            res.OutTradeNo,
            res.RefundId,
            RevertAmount(res.Amount.Refund),
            RevertAmount(res.Amount.Total));

        return refundResult;
    }

    //public override Task<UserAgreement> GetContractOrderPageUrl(
    //    MerchantDeductionAgreementConfiguration configuration,
    //    WeChatPayTradeType tradeType,
    //    string agreementNo,
    //    string orderNumber,
    //    string title,
    //    decimal firstAmount,
    //    string ip,
    //    DateTimeOffset timeExpire,
    //    string displayAccount,
    //    string? userOpenId,
    //    string? returnUrl)
    //{
    //    throw new NotImplementedException("无此接口的v3版本");
    //}

    //public override Task<bool> UnSignAgreement(string contractId)
    //{
    //    throw new NotImplementedException("无此接口的v3版本");
    //}

    //public override Task<bool> ExecuteAgreementDeduction(string orderNumber, string title, decimal amount, string agreementNo)
    //{
    //    throw new NotImplementedException("无此接口的v3版本");
    //}
}

