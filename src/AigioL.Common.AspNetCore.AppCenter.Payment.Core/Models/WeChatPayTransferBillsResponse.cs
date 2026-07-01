using System.Text.Json.Serialization;

using Essensoft.Paylink.WeChatPay;
using Essensoft.Paylink.WeChatPay.V3;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Models;

/// <summary>
/// 微信商家转账 - 发起转账响应
/// </summary>
public sealed class WeChatPayTransferBillsResponse : WeChatPayResponse
{
    /// <summary>
    /// 商户单号
    /// </summary>
    [JsonPropertyName("out_bill_no")]
    public string OutBillNo { get; set; } = string.Empty;

    /// <summary>
    /// 微信转账单号
    /// </summary>
    [JsonPropertyName("transfer_bill_no")]
    public string TransferBillNo { get; set; } = string.Empty;

    /// <summary>
    /// 单据创建时间
    /// </summary>
    [JsonPropertyName("create_time")]
    public string CreateTime { get; set; } = string.Empty;

    /// <summary>
    /// 单据状态
    /// ACCEPTED/PROCESSING/WAIT_USER_CONFIRM/TRANSFERING/SUCCESS/FAIL/CANCELING/CANCELLED
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// 跳转领取页面的package信息（仅WAIT_USER_CONFIRM时返回）
    /// </summary>
    [JsonPropertyName("package_info")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PackageInfo { get; set; }
}
