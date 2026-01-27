using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using Essensoft.Paylink.Alipay;
using Essensoft.Paylink.Alipay.Domain;
using Essensoft.Paylink.Alipay.Request;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Web;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Services;

sealed partial class AliPayServices<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
    ILogger<IAliPayServices> logger,
    IAlipayClient client,
    IOptions<TAppSettings> options,
    IOptionsFactory<AlipayExOptions> alipayOptionsFactory) : IAliPayServices
    where TAppSettings : class, IPaySettings
{
    readonly IAlipayClient client = client;
    readonly AlipayExOptions paymentOptions = alipayOptionsFactory.Create(Options.DefaultName), paymentMiniOptions = alipayOptionsFactory.Create("Mini");
    readonly JsonSerializerOptions jsonSerializerOptions = GetJsonSerializerOptions();

    public bool DebugOnlinePayment => options.Value.DebugOnlinePayment;

    public async Task<PubPayState> PubPay(AliPayPayTradeType tradeType, string orderId, string orderNumber, string title, decimal amount, string body, DateTimeOffset timeExpire, string? returnUrl = null)
    {
        var amountStr = FormatAmount(amount);

        switch (tradeType)
        {
            case AliPayPayTradeType.JSAPI_PC:
                var pagePayModel = new AlipayTradePagePayModel
                {
                    OutTradeNo = orderNumber,
                    Subject = title,
                    ProductCode = "FAST_INSTANT_TRADE_PAY",
                    TotalAmount = amountStr,
                    Body = body,
                    TimeExpire = timeExpire.ToOffset(TimeZoneInfo.Local.BaseUtcOffset).ToString("yyyy-MM-dd HH:mm:ss"),
                };

                var pcJsPayRequest = new AlipayTradePagePayRequest();
                pcJsPayRequest.SetBizModel(pagePayModel);
                pcJsPayRequest.SetNotifyUrl(paymentOptions.NotifyUrl);
                pcJsPayRequest.SetReturnUrl(returnUrl ?? $"{paymentOptions.ReturnUrl}?id={orderId}");

                LogApiInfo(pagePayModel);
                var pcJsResponse = await client.PageExecuteAsync(pcJsPayRequest, paymentOptions, null, "GET");
                LogApiInfo(pagePayModel.OutTradeNo, pagePayModel, pcJsResponse);

                return pcJsResponse.IsError
                    ? new() { IsSuccess = false, Message = pcJsResponse.Msg }
                    : new() { IsSuccess = true, Url = pcJsResponse.Body };

            case AliPayPayTradeType.ScanQRCodes:
                var preCreateModel = new AlipayTradePrecreateModel
                {
                    OutTradeNo = orderNumber,
                    Subject = title,
                    TotalAmount = amountStr,
                    Body = body,
                    QrCodeTimeoutExpress = Math.Floor(timeExpire.Subtract(DateTimeOffset.Now).TotalMinutes) + "m"
                };
                var qrPayRequest = new AlipayTradePrecreateRequest();
                qrPayRequest.SetBizModel(preCreateModel);
                qrPayRequest.SetNotifyUrl(paymentOptions.NotifyUrl);

                LogApiInfo(preCreateModel);
                var preCreateResponse = await client.RequestExecuteAsync(qrPayRequest, paymentOptions);
                LogApiInfo(preCreateModel.OutTradeNo, preCreateModel, preCreateResponse);

                return preCreateResponse.IsError
                    ? new() { IsSuccess = false, Message = preCreateResponse.Msg }
                    : new() { IsSuccess = true, Url = preCreateResponse.QrCode };

            case AliPayPayTradeType.MWEB:
                var wapPayModel = new AlipayTradeWapPayModel
                {
                    OutTradeNo = orderNumber,
                    Subject = title,
                    ProductCode = "QUICK_WAP_PAY",
                    TotalAmount = amountStr,
                    Body = body,
                    TimeExpire = timeExpire.ToOffset(TimeZoneInfo.Local.BaseUtcOffset).ToString("yyyy-MM-dd HH:mm:ss"),
                };
                var wapPayRequest = new AlipayTradeWapPayRequest();
                wapPayRequest.SetBizModel(wapPayModel);
                wapPayRequest.SetNotifyUrl(paymentOptions.NotifyUrl);

                LogApiInfo(wapPayModel);
                var wapPayResponse = await client.PageExecuteAsync(wapPayRequest, paymentOptions, null, "GET");
                LogApiInfo(wapPayModel.OutTradeNo, wapPayModel, wapPayResponse);

                return wapPayResponse.IsError
                    ? new() { IsSuccess = false, Message = wapPayResponse.Msg }
                    : new() { IsSuccess = true, Url = wapPayResponse.Body };

            default:
                return new() { IsSuccess = false, Message = "不支持的交易类型" };

                //case AliPayPayTradeType.JSAPI:
                //    break;

                //APP待支持
                //case AliPayPayTradeType.APP:
                //    var appPayModel = new AlipayTradeAppPayModel
                //    {
                //        OutTradeNo = orderNumber,
                //        Subject = title,
                //        ProductCode = "QUICK_MSECURITY_PAY",
                //        TotalAmount = amountStr,
                //        Body = body
                //    };
                //    var appPayRequest = new AlipayTradeAppPayRequest();
                //    appPayRequest.SetBizModel(appPayModel);
                //    appPayRequest.SetNotifyUrl(notifyUrl);
                //    var appPayResponse = await client.SdkExecuteAsync(appPayRequest, payOptions);
                //    state.IsSuccess = !appPayResponse.IsError;
                //    if (state.IsSuccess)
                //    {
                //        state.Message = appPayResponse.Msg;
                //    }
                //    else
                //    {
                //        state.Url = appPayResponse.Body;
                //    }
                //    return state;
        }
    }

    public async Task<(bool Success, bool RefundSuccess, string Code, string ErrorDesc)> Refund(string orderNumber, string refundNumber, decimal amount)
    {
        var amountStr = FormatAmount(amount);

        var model = new AlipayTradeRefundModel
        {
            OutTradeNo = orderNumber,
            OutRequestNo = refundNumber,
            RefundAmount = amountStr,
        };

        var req = new AlipayTradeRefundRequest();
        req.SetNotifyUrl(paymentOptions.NotifyUrl);
        req.SetBizModel(model);

        LogApiInfo(model);
        var res = await client.RequestExecuteAsync(req, paymentOptions);
        LogApiInfo(model.OutTradeNo, model, res);

        if (res.IsError)
            return (false, false, res.SubCode, res.SubMsg);

        return res.FundChange == "Y"
            ? (true, true, res.SubCode, res.SubMsg)
            : (true, false, res.SubCode, res.SubMsg);
    }

    public async Task<bool?> TradeClose(string orderNumber)
    {
        var model = new AlipayTradeCloseModel
        {
            OutTradeNo = orderNumber,
        };

        var req = new AlipayTradeCloseRequest();
        req.SetBizModel(model);
        req.SetNotifyUrl(paymentOptions.NotifyUrl);

        LogApiInfo(model);
        var res = await client.RequestExecuteAsync(req, paymentOptions);
        LogApiInfo(model.OutTradeNo, model, res);

        return !res.IsError;
    }

    public async Task<PubTransferState> Transfer(string outBizNo, decimal transAmount, string title, string userOpenId)
    {
        var model = new AlipayFundTransUniTransferModel
        {
            OutBizNo = outBizNo,
            TransAmount = FormatAmount(transAmount),
            BizScene = "DIRECT_TRANSFER",
            ProductCode = "TRANS_ACCOUNT_NO_PWD",
            OrderTitle = title,
            PayeeInfo = new()
            {
                Identity = userOpenId,
                IdentityType = "ALIPAY_OPEN_ID",
            },
        };

        var req = new AlipayFundTransUniTransferRequest();
        req.SetBizModel(model);
        req.SetNotifyUrl(paymentOptions.NotifyUrl);

        LogApiInfo(model);
        var res = await client.CertificateExecuteAsync(req, paymentOptions);
        LogApiInfo(model.OutBizNo, model, res);

        return new()
        {
            IsSuccess = !res.IsError,
            Message = res.SubMsg,
            ThirdPartyPlatformNumber = res.OrderId,
            AlipayPayFundOrderId = res.PayFundOrderId,
            TransferStatus = !res.IsError ? TransferStatus.Success : TransferStatus.Failed,
            FinishTime = res.TransDate != null ? DateTimeOffset.Parse(res.TransDate) : null,
        };
    }

    public async Task<AliPayTradeResult?> TradeQuery(string orderNumber)
    {
        var model = new AlipayTradeQueryModel { OutTradeNo = orderNumber };
        var req = new AlipayTradeQueryRequest();
        req.SetBizModel(model);

        var res = await client.RequestExecuteAsync(req, paymentOptions);
        if (res.IsError)
            return null;

        var tradeStatus = GetTradeStatus(res.TradeStatus);
        if (tradeStatus == null)
            return null;

        var tradeResult = new AliPayTradeResult(
            res.OutTradeNo,
            res.TradeNo,
            res.BuyerUserId,
            res.BuyerLogonId,
            decimal.Parse(res.BuyerPayAmount),
            decimal.Parse(res.TotalAmount),
            decimal.Parse(res.ReceiptAmount),
            DateTimeOffset.Parse(res.SendPayDate),
            tradeStatus.Value);

        return tradeResult;
    }

    public async Task<RefundResult?> RefundQuery(string orderNumber, string refundNumber)
    {
        var model = new AlipayTradeFastpayRefundQueryModel
        {
            OutTradeNo = orderNumber,
            OutRequestNo = refundNumber,
        };

        var req = new AlipayTradeFastpayRefundQueryRequest();
        req.SetBizModel(model);

        var res = await client.RequestExecuteAsync(req, paymentOptions);
        if (res.IsError)
            return null;

        var refundResult = new RefundResult(
            res.OutRequestNo,
            res.OutTradeNo,
            res.TradeNo,
            decimal.Parse(res.TotalAmount),
            decimal.Parse(res.RefundAmount));

        return refundResult;
    }

    public async Task<UserAgreement> GetAgreementSignPageUrl(MerchantDeductionAgreementConfiguration configuration, string outAgreementNo)
    {
        var executeTime = GetExecuteTime(configuration.PeriodType);

        var model = new AlipayUserAgreementPageSignModel
        {
            ProductCode = "GENERAL_WITHHOLDING",
            PersonalProductCode = "CYCLE_PAY_AUTH_P",
            SignScene = configuration.SignScene,
            ExternalAgreementNo = outAgreementNo,
            AccessParams = new() { Channel = "ALIPAYAPP" },
            PeriodRuleParams = new()
            {
                PeriodType = configuration.PeriodType,
                Period = configuration.Period,
                ExecuteTime = executeTime.ToString("yyyy-MM-dd"),
                SingleAmount = FormatAmount(configuration.SingleAmount),
                TotalPayments = 999, // 不传会提示商家参数错误
            },
        };

        var req = new AlipayUserAgreementPageSignRequest();
        req.SetBizModel(model);
        req.SetNotifyUrl(paymentOptions.NotifyUrl);

        LogApiInfo(model);
        var res = await client.PageExecuteAsync(req, paymentOptions, null, "GET");
        LogApiInfo(model.ExternalAgreementNo, model, res);

        return new UserAgreement(res.Body, executeTime);
    }

    public async Task<UserAgreement> GetPayAndAgreementSignPageUrl(
        MerchantDeductionAgreementConfiguration configuration,
        string agreementNo,
        string orderNumber,
        string title,
        decimal firstAmount,
        string ip,
        DateTimeOffset timeExpire,
        string displayAccount,
        string? userOpenId = null,
        string? returnUrl = null)
    {
        var executeTime = DateTime.Today;
        var amountStr = FormatAmount(firstAmount);

        var model = new AlipayTradeSignCreateModel
        {
            OutTradeNo = orderNumber,
            Subject = title,
            ProductCode = "JSAPI_PAY",
            TotalAmount = amountStr,
            BuyerOpenId = userOpenId,
            TimeExpire = timeExpire.ToOffset(TimeZoneInfo.Local.BaseUtcOffset).ToString("yyyy-MM-dd HH:mm:ss"),
            AgreementSignParams = new()
            {
                AccessParams = new() { Channel = "ALIPAYAPP" },
                PeriodRuleParams = new()
                {
                    PeriodType = configuration.PeriodType,
                    Period = configuration.Period,
                    ExecuteTime = executeTime.ToString("yyyy-MM-dd"),
                    SingleAmount = amountStr,
                    TotalPayments = 999, // 不传会提示商家参数错误
                },
                SignNotifyUrl = paymentMiniOptions.NotifyUrl,
                ExternalLogonId = displayAccount,
                PersonalProductCode = "CYCLE_PAY_AUTH_P",
                ExternalAgreementNo = agreementNo,
                ProductCode = "GENERAL_WITHHOLDING",
                SignScene = "INDUSTRY|WATT_MONTH",
            },
        };

        var req = new AlipayTradeCreateRequest();
        req.SetBizModel(model);
        req.SetNotifyUrl(paymentMiniOptions.NotifyUrl);

        LogApiInfo(model);
        var res = await client.RequestExecuteAsync(req, paymentMiniOptions);
        LogApiInfo(model.AgreementSignParams.ExternalAgreementNo, model, res);

        if (res.IsError)
            return UserAgreement.Error(res.SubMsg);

        string appId = paymentMiniOptions.AppId;

        // refer to https://opensupport.alipay.com/support/FAQ/673b09dff06acf044690d7beprod
        var mpPageQuery = HttpUtility.UrlEncode("tradeNO=" + res.TradeNo);
        var mpSchemeLink = HttpUtility.UrlEncode($"alipays://platformapi/startapp?appId={appId}&page=pages/trade-pay/trade-pay&query=" + mpPageQuery);
        var url = "https://ds.alipay.com/?scheme=" + mpSchemeLink;

        return new UserAgreement(url, executeTime) { TradeNo = res.TradeNo };
    }

    public async Task<bool> UnSignAgreement(string agreementNo)
    {
        var model = new AlipayUserAgreementUnsignModel
        {
            AgreementNo = agreementNo,
        };

        var req = new AlipayUserAgreementUnsignRequest();
        req.SetBizModel(model);
        req.SetNotifyUrl(paymentOptions.NotifyUrl);

        LogApiInfo(model);
        var res = await client.RequestExecuteAsync(req, paymentOptions);
        LogApiInfo(agreementNo, model, res);

        return !res.IsError;
    }

    public async Task<bool> DelayAgreementExecutionPlan(string agreementNo, DateTimeOffset deductTime, string memo)
    {
        var model = new AlipayUserAgreementExecutionplanModifyModel
        {
            AgreementNo = agreementNo,
            DeductTime = deductTime.ToString("yyyy-MM-dd"),
            Memo = memo,
        };

        var req = new AlipayUserAgreementExecutionplanModifyRequest();
        req.SetBizModel(model);
        req.SetNotifyUrl(paymentOptions.NotifyUrl);

        LogApiInfo(model);
        var res = await client.RequestExecuteAsync(req, paymentOptions);
        LogApiInfo(model.AgreementNo, model, res);
        return !res.IsError;
    }

    public async Task<bool> ExecuteAgreementDeduction(string orderNumber, string title, decimal amount, string agreementNo)
    {
        var model = new AlipayTradePayModel
        {
            OutTradeNo = orderNumber,
            ProductCode = "GENERAL_WITHHOLDING",
            TotalAmount = FormatAmount(amount),
            Subject = title,
            AgreementParams = new() { AgreementNo = agreementNo },
        };

        var req = new AlipayTradePayRequest();
        req.SetBizModel(model);
        req.SetNotifyUrl(paymentOptions.NotifyUrl);

        LogApiInfo(model);
        var res = await client.RequestExecuteAsync(req, paymentOptions);
        LogApiInfo(model.OutTradeNo, model, res);

        return res is
        {
            Code: DeductResponseCode.支付成功 or
                  DeductResponseCode.等待用户付款 or
                  DeductResponseCode.未知异常
        };
    }

    public async Task<string?> GetUserOpenId(string code)
    {
        /*
        {
            "alipay_system_oauth_token_response": {
                "access_token": "authbseB4e422c6b9dcb462bbae57d4715603X11",
                "auth_start": "2024-12-06 11:20:50",
                "expires_in": 31536000,
                "re_expires_in": 31536000,
                "refresh_token": "authbseB1b529015be9449c6b5f3a0f4335ffD11",
                "open_id": "011b9pIPE9AmZCILiLrs3Ej8avi4H6XMCXgXIzmYt-B3OAa"
            },
            "sign": "N/CwE5dcMY0UIVR445AruphnrRLwMeQ6eu0Ldz..."
        }
         */
        var request = new AlipaySystemOauthTokenRequest() { Code = code, GrantType = "authorization_code" };
        var response = await client.RequestExecuteAsync(request, paymentMiniOptions);
        var userOpenId = JsonNode.Parse(response.Body)?.AsObject()["alipay_system_oauth_token_response"]?["open_id"]?.GetValue<string>();
        return userOpenId;
    }

    public string GetMiniProgramPayUrl(string continueUrl, string agreementNo)
    {
        string appId = paymentMiniOptions.AppId;
        var mpPageQuery = HttpUtility.UrlEncode($"?state={agreementNo}&continueUrl={HttpUtility.UrlEncode(continueUrl)}");
        var mpSchemeLink = HttpUtility.UrlEncode($"alipays://platformapi/startapp?appId={appId}&page=pages/trade-pay/trade-pay" + mpPageQuery);
        var url = "https://ds.alipay.com/?scheme=" + mpSchemeLink;
        return url;
    }
}

partial class AliPayServices<TAppSettings>
{
    static TradeStatus? GetTradeStatus(string tradeStatus) => tradeStatus switch
    {
        AlipayTradeStatus.Wait => TradeStatus.WaitBuyerPay,
        AlipayTradeStatus.Closed => TradeStatus.TradeClosed,
        AlipayTradeStatus.Success => TradeStatus.TradeSuccess,
        AlipayTradeStatus.Finished => TradeStatus.TradeFinished,
        _ => null,
    };

    DateTimeOffset GetExecuteTime(string periodType)
    {
        var executeDay = (DateTimeOffset)DateTime.Today;

        // 如果时间离明天不到30分钟，则扣款日使用明天，因为执行初次扣款时如果已经到第二天了就会失败
        if (DateTime.Today.AddDays(1).Subtract(DateTime.Now) < TimeSpan.FromMinutes(30))
        {
            executeDay = executeDay.AddDays(1);
        }

        if (periodType == "MONTH")
        {
            if (executeDay.Day > 28)
            {
                // 计划扣款时间 execute_time 不允许传28日之后的日期（可以传28日），以此避免有些月份可能不存在对应日期的情况。
                // 如果今天是28，那么 execute_time 为下月1号，那么就不能扣款日当天提前一天，提前1天会导致29-31没有会员
                // https://opendocs.alipay.com/support/01rg2d?ant_source=opendoc_recommend
                executeDay = new DateTime(executeDay.Year, executeDay.Month, 1).AddMonths(1);
            }
        }

        return executeDay;
    }

    string FormatAmount(decimal amount)
    {
        if (DebugOnlinePayment)
            return "0.01";

        amount = Math.Round(amount, 2, MidpointRounding.ToEven);
        return amount.ToString();
    }

    static JsonSerializerOptions GetJsonSerializerOptions()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        o.TypeInfoResolverChain.Insert(0, AliPayServicesJsonSerializerContext.Default);
        o.TypeInfoResolverChain.Insert(1, PaymentMinimalApisJsonSerializerContext.Default);
        return o;
    }

    void LogApiInfo<T>(T requestModel) where T : AlipayObject
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Request {RequestModel}", JsonSerializer.Serialize(requestModel, jsonSerializerOptions.GetTypeInfo(typeof(T))));
        }
    }

    void LogApiInfo<T>(string requestIdentifier, T requestModel, AlipayResponse response) where T : AlipayObject
    {
        if (response.IsError && logger.IsEnabled(LogLevel.Error))
        {
            var jsonRequestModel = JsonSerializer.Serialize(requestModel, jsonSerializerOptions.GetTypeInfo(typeof(T)));
            logger.LogError("Request {RequestModel} Response {ResponseBody}", jsonRequestModel, response.Body);
        }
        else if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("RequestIdentifier {RequestIdentifier} Response {ResponseBody}", requestIdentifier, response.Body);
        }
    }
}

/// <summary>
/// 根据公共返回参数中的 code，这笔交易可能有四种状态：支付成功（10000），支付失败（40004），等待用户付款（10003）和未知异常（20000）。
/// 对于扣款失败或未知异常， 有如下解决方案：
///     建议商家等待5分钟后，使用alipay.trade.query（统一收单交易查询接口）查询交易状态，如果交易状态是成功，则无需特殊处理；
///     若查询 4 次后依然返回等待用户付款，商户可以选择幂等重试扣款或通过 alipay.trade.cancel（统一收单交易撤销接口）发起交易撤销。
/// </summary>
file static partial class DeductResponseCode
{
    /// <summary>
    /// 记录交易结果并在显示支付成功，进入后续的业务处理。
    /// </summary>
    public const string 支付成功 = "10000";

    /// <summary>
    /// 记录交易结果并显示错误信息（display_message）。
    /// </summary>
    public const string 支付失败 = "40004";

    /// <summary>
    /// 一般等待5分钟后调用交易查询接口 alipay.trade.query。 通过支付时传入的商户订单号（out_trade_no）查询支付结果（返回参数 TRADE_STATUS ）。
    /// 查询4次之后若仍然返回等待用户付款（WAIT_BUYER_PAY），商户可以选择幂等重试代扣或者发起交易撤销 alipay.trade.cancel。
    /// </summary>
    public const string 等待用户付款 = "10003";

    /// <summary>
    /// 一般等待5分钟后调用交易查询接口 alipay.trade.query。 通过支付时传入的商户订单号（out_trade_no）查询支付结果（返回参数 TRADE_STATUS ）。
    /// 查询4次之后若仍然返回等待用户付款（WAIT_BUYER_PAY），商户可以选择幂等重试代扣或者发起交易撤销 alipay.trade.cancel。
    /// </summary>
    public const string 未知异常 = "20000";
}

public sealed partial class AlipayTradeSignCreateModel : AlipayTradeCreateModel
{
    /// <summary>
    /// 【描述】买家支付宝用户唯一标识 详情可查看 openid简介
    /// 【示例值】074a1CcTG1LelxKe4xQC0zgNdId0nxi95b5lsNpazWYoCo5
    /// </summary>
    [JsonPropertyName("buyer_open_id")]
    public string? BuyerOpenId { get; set; }

    /// <summary>
    /// 【描述】订单绝对超时时间。
    /// 格式为yyyy-MM-dd HH:mm:ss。
    /// 注：time_expire和timeout_express两者只需传入一个或者都不传，如果两者都传，优先使用time_expire。
    /// </summary>
    [JsonPropertyName("time_expire")]
    public string? TimeExpire { get; set; }

    /// <summary>
    /// 签约参数，支付后签约场景使用
    /// </summary>
    [JsonPropertyName("agreement_sign_params")]
    public FixSignParams AgreementSignParams { get; set; } = new();

    public sealed partial class FixSignParams : SignParams
    {
        /// <summary>
        /// 商家和支付宝签约的产品码。 商家扣款产品传入固定值：GENERAL_WITHHOLDING
        /// </summary>
        [JsonPropertyName("product_code")]
        public string ProductCode { get; set; } = string.Empty;
    }
}

[JsonSerializable(typeof(AlipayTradePagePayModel))]
[JsonSerializable(typeof(AlipayTradePrecreateModel))]
[JsonSerializable(typeof(AlipayTradeWapPayModel))]
[JsonSerializable(typeof(AlipayTradeRefundModel))]
[JsonSerializable(typeof(AlipayTradeCloseModel))]
[JsonSerializable(typeof(AlipayUserAgreementUnsignModel))]
[JsonSerializable(typeof(AlipayUserAgreementExecutionplanModifyModel))]
[JsonSerializable(typeof(AlipayTradePayModel))]
[JsonSerializable(typeof(AlipayFundTransUniTransferModel))]
[JsonSerializable(typeof(AlipayTradeQueryModel))]
[JsonSerializable(typeof(AlipayTradeFastpayRefundQueryModel))]
[JsonSerializable(typeof(AlipayUserAgreementPageSignModel))]
[JsonSerializable(typeof(AlipayTradeSignCreateModel))]
[JsonSourceGenerationOptions]
internal sealed partial class AliPayServicesJsonSerializerContext : JsonSerializerContext
{
    static AliPayServicesJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new AliPayServicesJsonSerializerContext(o);
    }
}