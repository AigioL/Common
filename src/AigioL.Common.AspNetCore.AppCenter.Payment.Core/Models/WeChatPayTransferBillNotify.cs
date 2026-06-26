using System.Text.Json.Serialization;

using Essensoft.Paylink.WeChatPay.V3;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Models;

/// <summary>
/// 微信商家转账回调通知 - 解密后的资源数据
/// </summary>
public sealed class WeChatPayTransferBillNotify : WeChatPayNotify
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
    /// 单据状态：SUCCESS/FAIL/CANCELLED
    /// </summary>
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// 商户号
    /// </summary>
    [JsonPropertyName("mch_id")]
    public string MchId { get; set; } = string.Empty;

    /// <summary>
    /// 转账金额（单位：分）
    /// </summary>
    [JsonPropertyName("transfer_amount")]
    public int TransferAmount { get; set; }

    /// <summary>
    /// 收款用户OpenID
    /// </summary>
    [JsonPropertyName("openid")]
    public string OpenId { get; set; } = string.Empty;

    /// <summary>
    /// 失败原因（终态为FAIL时返回）
    /// </summary>
    [JsonPropertyName("fail_reason")]
    public string? FailReason { get; set; }

    /// <summary>
    /// 单据创建时间
    /// </summary>
    [JsonPropertyName("create_time")]
    public string CreateTime { get; set; } = string.Empty;

    /// <summary>
    /// 最后一次状态变更时间
    /// </summary>
    [JsonPropertyName("update_time")]
    public string UpdateTime { get; set; } = string.Empty;
}
