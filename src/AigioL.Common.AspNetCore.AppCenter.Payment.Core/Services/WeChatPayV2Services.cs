using AigioL.Common.AspNetCore.AppCenter.Helpers.SnowFlake;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using Essensoft.Paylink.WeChatPay;
using Essensoft.Paylink.WeChatPay.V2;
using Essensoft.Paylink.WeChatPay.V2.Request;
using Essensoft.Paylink.WeChatPay.V2.Response;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Xml.Serialization;
using WeChatPayTradeType = global::AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment.WeChatPayTradeType;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Services;

partial class WeChatPayV2Services<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
    IServiceProvider serviceProvider,
    IOptions<TAppSettings> options,
    IOptions<WeChatPayExOptions> weChatPayOptions,
    ILogger<IWeChatPayServices> logger) : WeChatPayServices<TAppSettings>(serviceProvider, options, weChatPayOptions, logger)
    where TAppSettings : class, IPaySettings
{
    IWeChatPayClient ClientV2 => field ??= serviceProvider.GetRequiredService<IWeChatPayClient>();

    public virtual async Task<UserAgreement> GetContractOrderPageUrl(
        MerchantDeductionAgreementConfiguration configuration,
        WeChatPayTradeType tradeType,
        string agreementNo,
        string orderNumber,
        string title,
        decimal firstAmount,
        string ip,
        DateTimeOffset timeExpire,
        string displayAccount,
        string? userOpenId,
        string? returnUrl)
    {
        var orderRequest = new WeChatPayContractOrderRequest
        {
            Body = title,
            OutTradeNo = orderNumber,
            TotalFee = FormatAmount(firstAmount),
            SpBillCreateIp = ip,
            NotifyUrl = PaymentOptions.NotifyUrl,
            TradeType = (tradeType == WeChatPayTradeType.JSAPI_OFFICIAL ? WeChatPayTradeType.JSAPI : tradeType).ToString(),
            OpenId = userOpenId, // JSAPI支付时，此参数必传
            TimeExpire = timeExpire.ToString("yyyyMMddHHmmss"),
            PlanId = configuration.PlanId!,
            ContractCode = agreementNo,
            RequestSerial = IdGeneratorHelper.GetNextId(),
            ContractDisplayAccount = displayAccount,
            ContractNotifyUrl = PaymentOptions.ContractNotifyUrl,
        };

        LogApiInfo(orderRequest.GetParameters());
        var response = await ClientV2.ExecuteAsync<WeChatPayContractOrderResponse>(orderRequest, PaymentOptions);
        LogApiInfo(orderRequest.OutTradeNo, orderRequest, response, response.ReturnCode == WeChatPayCode.Success);

        switch (tradeType)
        {
            case WeChatPayTradeType.MWEB:
                return response.MwebUrl != null
                    ? new(response.MwebUrl, DateTime.Today)
                    : UserAgreement.Error("找不到网页 Url");

            case WeChatPayTradeType.NATIVE:
                return response.CodeUrl != null && response.CodeUrl.Contains("weixin://wxpay/bizpayurl?pr=")
                    ? new(response.CodeUrl, DateTime.Today)
                    : UserAgreement.Error("找不到二维码 Url");

            case WeChatPayTradeType.JSAPI_OFFICIAL:
                if (string.IsNullOrEmpty(response.PrepayId))
                    return UserAgreement.Error("缺少预支付交易会话标识 PrepayId");

                var req = new WeChatPayJsApiSdkRequest
                {
                    Package = "prepay_id=" + response.PrepayId,
                };

                LogApiInfo(req.GetParameters());
                var parameter = await ClientV2.ExecuteAsync(req, PaymentOptions);
                var urlValid = TryBuildPaymentPageUrl(returnUrl, parameter, out var paymentPageUrl);
                LogApiInfo(req.Package, req.GetParameters(), parameter, urlValid);

                return urlValid
                    ? new(paymentPageUrl, DateTime.Today)
                    : UserAgreement.Error("获取 JsApi 参数失败");

            default:
                return UserAgreement.Error("不支持的交易类型");
        }
    }

    public virtual async Task<bool> UnSignAgreement(string contractId)
    {
        var request = new WeChatPayPaPayDeleteContractRequest
        {
            ContractId = contractId,
            ContractTerminationRemark = "申请解约",
        };

        LogApiInfo(request.GetParameters());
        var response = await ClientV2.ExecuteAsync(request, PaymentOptions);
        LogApiInfo(request.ContractId, request, response, response.ReturnCode == WeChatPayCode.Success);

        if (response.ReturnCode == WeChatPayCode.Success && response.ResultCode == WeChatPayCode.Success)
            return true;

        logger.LogError("微信解约委托代扣协议错误 {ErrCodeDes}", response.ErrCodeDes);
        return false;
    }

    public virtual async Task<bool> ExecuteAgreementDeduction(string orderNumber, string title, decimal amount, string agreementNo)
    {
        var req = new WeChatPayPartnerPapPayApplyRequest
        {
            Body = title,
            OutTradeNo = orderNumber,
            TotalFee = FormatAmount(amount),
            NotifyUrl = PaymentOptions.NotifyUrl,
            TradeType = "PAP",
            ContractId = agreementNo,
        };

        LogApiInfo(req.GetParameters());
        var response = await ClientV2.ExecuteAsync(req, PaymentOptions);
        LogApiInfo(req.OutTradeNo, req, response, response.ReturnCode == WeChatPayCode.Success);
        if (response.ReturnCode == WeChatPayCode.Success && response.ResultCode == WeChatPayCode.Success)
            return true;

        logger.LogError("微信执行扣款错误 {ErrCodeDes}", response.ErrCodeDes);
        return false;
    }

    bool TryBuildPaymentPageUrl(string? returnUrl, WeChatPayDictionary parameter, out string paymentPageUrl)
    {
        string[] parameters = ["appId", "timeStamp", "nonceStr", "package", "signType", "paySign"];
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
        var ub = new UriBuilder(OfficialUrl) { Path = "wechatpay", Query = qs.ToString(), };
        paymentPageUrl = ub.ToString();
        return true;
    }
}

/// <summary>
/// 扣款服务-直连模式 - 支付中签约
/// </summary>
public sealed partial class WeChatPayContractOrderRequest : WeChatPayUnifiedOrderRequest, IWeChatPayRequest<WeChatPayContractOrderResponse>
{
    /// <summary>
    /// 模板id
    /// </summary>
    /// <remarks>
    /// 必填：是
    /// 描述：协议模板id。
    /// 示例值：123
    /// </remarks>
    public string PlanId { get; init; } = string.Empty;

    /// <summary>
    /// 签约协议号
    /// </summary>
    /// <remarks>
    /// 长度：[1,64]
    /// 必填：是
    /// 描述：商户侧的签约协议号，由商户生成，只能是数字、大小写字母的描述。
    /// 示例值：100001256
    /// </remarks>
    public string ContractCode { get; init; } = string.Empty;

    /// <summary>
    /// 请求序列号
    /// </summary>
    /// <remarks>
    /// 长度： 64
    /// 必填：是
    /// 描述：商户请求签约时的序列号，要求唯一性。禁止使用0开头，序列号主要用于排序，不作为查询条件，纯数字，范围不能超过int 64的范围（9223372036854775807）。
    /// 示例值：1695
    /// </remarks>
    public string RequestSerial { get; init; } = string.Empty;

    /// <summary>
    /// 用户账户展示名称
    /// </summary>
    /// <remarks>
    /// 长度：[1,64]
    /// 必填：是
    /// 描述：签约用户的名称,用于页面展示，参数值不支持UTF8非3字节编码的字符，例如表情符号，所以请勿传微信昵称到该字段。
    /// 示例值：123
    /// </remarks>
    public string ContractDisplayAccount { get; init; } = string.Empty;

    /// <summary>
    /// 签约信息通知url
    /// </summary>
    /// <remarks>
    /// 长度：[1,256]
    /// 必填：是
    /// 描述：签约信息回调通知的url，以http或https开头，通知url必须为外网可访问的url，不能携带参数。
    /// 示例值：https://yoursite.com
    /// </remarks>
    public string ContractNotifyUrl { get; init; } = string.Empty;

    #region IWeChatPayRequest Members

    private string requestUrl = "https://api.mch.weixin.qq.com/pay/contractorder";

    public new string GetRequestUrl() => requestUrl;

    public new void SetRequestUrl(string url) => requestUrl = url;

    public new IDictionary<string, string> GetParameters()
    {
        var parameters = new WeChatPayDictionary(base.GetParameters())
        {
            { "plan_id",  PlanId },
            { "contract_code",  ContractCode },
            { "request_serial",  RequestSerial },
            { "contract_display_account",  ContractDisplayAccount },
            { "contract_notify_url",  ContractNotifyUrl },
        };
        return parameters;
    }

    public new void PrimaryHandler(WeChatPayDictionary sortedTxtParams, WeChatPayOptions options)
    {
        sortedTxtParams.Add("contract_appid", options.AppId); // 签约公众号，必须与appid一致
        sortedTxtParams.Add("contract_mchid", options.MchId); // 签约商户号，必须与mch_id一致
        base.PrimaryHandler(sortedTxtParams, options);
    }

    #endregion IWeChatPayRequest Members
}

[XmlRoot("xml")]
public sealed partial class WeChatPayContractOrderResponse : WeChatPayUnifiedOrderResponse
{
    // 以下字段在 return_code 为 SUCCESS 的时候有返回

    /// <summary>
    /// 预签约结果
    /// </summary>
    /// <remarks>
    /// 必填：是
    /// 描述：预签约结果。
    /// 长度：[1,16]
    /// 示例值：SUCCESS
    /// </remarks>
    [XmlElement("contract_result_code")]
    public string? ContractResultCode { get; set; }

    /// <summary>
    /// 预签约错误代码
    /// </summary>
    /// <remarks>
    /// 必填：否
    /// 描述：预签约错误代码。
    /// 长度：[1,32]
    /// 示例值：Fail
    /// </remarks>
    [XmlElement("contract_err_code")]
    public string? ContractErrCode { get; set; }

    /// <summary>
    /// 预签约错误描述
    /// </summary>
    /// <remarks>
    /// 必填：否
    /// 描述：预签约错误描述。
    /// 长度：[1,32]
    /// 示例值：已签约
    /// </remarks>
    [XmlElement("contract_err_code_des")]
    public string? ContractErrCodeDes { get; set; }

    // 以下字段在 return_code 和 result_code 都为SUCCESS的时候有返回

    /// <summary>
    /// 模板id
    /// </summary>
    /// <remarks>
    /// 必填：否
    /// 描述：商户在微信商户平台设置的代扣协议模板id。
    /// 示例值：123
    /// </remarks>
    [XmlElement("plan_id")]
    public string? PlanId { get; set; }

    /// <summary>
    /// 请求序列号
    /// </summary>
    /// <remarks>
    /// 必填：否
    /// 描述：商户请求签约时的序列号,商户侧须唯一，禁止使用0开头。
    /// 长度： 64
    /// 示例值：1695
    /// </remarks>
    [XmlElement("request_serial")]
    public string? RequestSerial { get; set; }

    /// <summary>
    /// 签约协议号
    /// </summary>
    /// <remarks>
    /// 必填：否
    /// 描述：商户请求签约时传入的签约协议号,商户侧须唯一。
    /// 长度：[1,64]
    /// 示例值：1023658866
    /// </remarks>
    [XmlElement("contract_code")]
    public string? ContractCode { get; set; }

    /// <summary>
    /// 用户账户展示名称
    /// </summary>
    /// <remarks>
    /// 必填：否
    /// 描述：签约用户的名称,用于页面展示。
    /// 长度：[1,64]
    /// 示例值：张三
    /// </remarks>
    [XmlElement("contract_display_account")]
    public string? ContractDisplayAccount { get; set; }

    /// <summary>
    /// 商户订单号
    /// </summary>
    /// <remarks>
    /// 必填：是
    /// 描述：商户订单号。
    /// 长度：[1,32]
    /// 示例值：123456
    /// </remarks>
    [XmlElement("out_trade_no")]
    public string? OutTradeNo { get; set; }
}

/// <summary>
/// 微信代扣 - 申请解约 (服务商)
/// </summary>
/// <remarks>
/// 忽略 nonce_str 参数否则签名错误
/// </remarks>
public sealed partial class WeChatPayPaPayDeleteContractRequest : WeChatPayPaPayPartnerDeleteContractRequest, IWeChatPayRequest<WeChatPayPaPayPartnerDeleteContractResponse>
{
    public WeChatPayPaPayDeleteContractRequest()
    {
        Version = "1.0";
    }

    private const WeChatPaySignType signType = WeChatPaySignType.MD5;

    public new void PrimaryHandler(WeChatPayDictionary sortedTxtParams, WeChatPayOptions options)
    {
        sortedTxtParams.Add(WeChatPayConsts.appid, options.AppId);
        sortedTxtParams.Add(WeChatPayConsts.sub_appid, options.SubAppId);
        sortedTxtParams.Add(WeChatPayConsts.mch_id, options.MchId);
        sortedTxtParams.Add(WeChatPayConsts.sub_mch_id, options.SubMchId);

        sortedTxtParams.Add(WeChatPayConsts.sign, WeChatPaySignature.SignWithKey(sortedTxtParams, options.APIKey, signType));
    }
}