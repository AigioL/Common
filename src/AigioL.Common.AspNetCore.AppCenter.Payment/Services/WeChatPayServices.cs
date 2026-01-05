using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using Essensoft.Paylink.WeChatPay;
using Essensoft.Paylink.WeChatPay.V2.Request;
using Essensoft.Paylink.WeChatPay.V2.Response;
using Essensoft.Paylink.WeChatPay.V3;
using Essensoft.Paylink.WeChatPay.V3.Domain;
using Essensoft.Paylink.WeChatPay.V3.Request;
using Essensoft.Paylink.WeChatPay.V3.Response;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Services;

partial class WeChatPayServices<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
    IServiceProvider serviceProvider,
    IOptions<TAppSettings> options,
    IOptions<WeChatPayExOptions> weChatPayOptions,
    ILogger<IWeChatPayServices> logger)
    where TAppSettings : class, IPaySettings
{
    protected readonly WeChatPayExOptions PaymentOptions = weChatPayOptions.Value;
    protected readonly IServiceProvider serviceProvider = serviceProvider;
    protected readonly JsonSerializerOptions jsonSerializerOptions = GetJsonSerializerOptions();

    public bool DebugOnlinePayment => options.Value.DebugOnlinePayment;

    public string OfficialUrl => options.Value.OfficialUrl;
}

partial class WeChatPayServices<TAppSettings>
{
    protected static TradeStatus? GetTradeStatus(string tradeStatus) => tradeStatus switch
    {
        WeChatPayTradeState.Success => TradeStatus.TradeSuccess,
        WeChatPayTradeState.Refund => null,
        WeChatPayTradeState.NotPay => TradeStatus.WaitBuyerPay,
        WeChatPayTradeState.Closed => TradeStatus.TradeClosed,
        WeChatPayTradeState.Revoked => TradeStatus.WaitBuyerPay,
        WeChatPayTradeState.UserPaying => TradeStatus.WaitBuyerPay,
        WeChatPayTradeState.PayError => null,
        WeChatPayTradeState.Accept => null,
        _ => null,
    };

    protected int FormatAmount(decimal amount)
    {
        if (DebugOnlinePayment)
            return 1;

        return Convert.ToInt32(amount * 100);
    }

    protected decimal RevertAmount(int amountInCent)
    {
        return amountInCent / 100M;
    }

    static JsonSerializerOptions GetJsonSerializerOptions()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        o.TypeInfoResolverChain.Insert(0, WeChatPayServicesJsonSerializerContext.Default);
        o.TypeInfoResolverChain.Insert(1, PaymentMinimalApisJsonSerializerContext.Default);
        return o;
    }

    protected void LogApiInfo<T>(T requestModel)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            var jsonRequestModal = JsonSerializer.Serialize(requestModel, jsonSerializerOptions.GetTypeInfo(typeof(T)));
            logger.LogInformation("Request {RequestModel}", jsonRequestModal);
        }
    }

    protected void LogApiInfo<T, T2>(string requestIdentifier, T requestModel, T2 responseModel, bool success)
    {
        if (!success && logger.IsEnabled(LogLevel.Error))
        {
            var jsonRequestModal = JsonSerializer.Serialize(requestModel, jsonSerializerOptions.GetTypeInfo(typeof(T)));
            var jsonResponseModal = JsonSerializer.Serialize(responseModel, jsonSerializerOptions.GetTypeInfo(typeof(T2)));
            logger.LogError("Request {RequestModel} Response {ResponseBody}", jsonRequestModal, jsonResponseModal);
        }
        else if (logger.IsEnabled(LogLevel.Information))
        {
            var jsonResponseModal = JsonSerializer.Serialize(responseModel, jsonSerializerOptions.GetTypeInfo(typeof(T2)));
            logger.LogInformation("RequestIdentifier {RequestIdentifier} Response {ResponseBody}", requestIdentifier, jsonResponseModal);
        }
    }
}

[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(IDictionary<string, string>))]
[JsonSerializable(typeof(WeChatPayDictionary))]
[JsonSerializable(typeof(SortedDictionary<string, string>))]
[JsonSerializable(typeof(WeChatPayTransactionsH5BodyModel))]
[JsonSerializable(typeof(WeChatPayTransactionsNativeBodyModel))]
[JsonSerializable(typeof(WeChatPayRefundDomesticRefundsBodyModel))]
[JsonSerializable(typeof(WeChatPayTransactionsOutTradeNoCloseBodyModel))]
[JsonSerializable(typeof(WeChatPayContractOrderRequest))]
[JsonSerializable(typeof(WeChatPayContractOrderResponse))]
[JsonSerializable(typeof(WeChatPayPaPayDeleteContractRequest))]
[JsonSerializable(typeof(WeChatPayPaPayPartnerDeleteContractResponse))]
[JsonSerializable(typeof(WeChatPayPartnerPapPayApplyRequest))]
[JsonSerializable(typeof(WeChatPayPartnerPapPayApplyResponse))]
[JsonSerializable(typeof(WeChatPayTransactionsH5Request))]
[JsonSerializable(typeof(WeChatPayTransactionsH5Response))]
[JsonSerializable(typeof(WeChatPayTransactionsNativeRequest))]
[JsonSerializable(typeof(WeChatPayTransactionsNativeResponse))]
[JsonSerializable(typeof(WeChatPayTransactionsJsApiBodyModel))]
[JsonSerializable(typeof(WeChatPayTransactionsJsApiRequest))]
[JsonSerializable(typeof(WeChatPayTransactionsJsApiResponse))]
[JsonSerializable(typeof(WeChatPayRefundDomesticRefundsRequest))]
[JsonSerializable(typeof(WeChatPayRefundDomesticRefundsResponse))]
[JsonSerializable(typeof(WeChatPayTransactionsOutTradeNoCloseRequest))]
[JsonSerializable(typeof(WeChatPayTransactionsOutTradeNoCloseResponse))]
[JsonSourceGenerationOptions]
internal sealed partial class WeChatPayServicesJsonSerializerContext : JsonSerializerContext
{
    static WeChatPayServicesJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new WeChatPayServicesJsonSerializerContext(o);
    }
}