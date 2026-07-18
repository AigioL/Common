using Essensoft.Paylink.WeChatPay;
using Essensoft.Paylink.WeChatPay.V3;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Models;

/// <summary>
/// 微信商家转账 - 发起转账请求
/// </summary>
public sealed class WeChatPayTransferBillsRequest : IWeChatPayPostRequest<WeChatPayTransferBillsResponse>
{
    private WeChatPayObject? bodyModel;

    /// <inheritdoc/>
    public string GetRequestUrl() => "https://api.mch.weixin.qq.com/v3/fund-app/mch-transfer/transfer-bills";

    /// <inheritdoc/>
    public WeChatPayObject? GetBodyModel() => bodyModel;

    /// <inheritdoc/>
    public void SetBodyModel(WeChatPayObject bodyModel) => this.bodyModel = bodyModel;
}
