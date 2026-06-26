using System.Text.Json.Serialization;

using Essensoft.Paylink.WeChatPay;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Models;

/// <summary>
/// 微信商家转账 - 转账场景报备信息
/// </summary>
public sealed class WeChatPayTransferSceneReportInfo : WeChatPayObject
{
    /// <summary>
    /// 信息类型
    /// </summary>
    [JsonPropertyName("info_type")]
    public string InfoType { get; set; } = string.Empty;

    /// <summary>
    /// 信息内容
    /// </summary>
    [JsonPropertyName("info_content")]
    public string InfoContent { get; set; } = string.Empty;
}

/// <summary>
/// 微信商家转账 - 发起转账请求体
/// </summary>
public sealed class WeChatPayTransferBillsBodyModel : WeChatPayObject
{
    /// <summary>
    /// 商户AppID
    /// </summary>
    [JsonPropertyName("appid")]
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// 商户单号（32字符内，数字+大小写字母）
    /// </summary>
    [JsonPropertyName("out_bill_no")]
    public string OutBillNo { get; set; } = string.Empty;

    /// <summary>
    /// 转账场景ID（如：1000-现金营销）
    /// </summary>
    [JsonPropertyName("transfer_scene_id")]
    public string TransferSceneId { get; set; } = string.Empty;

    /// <summary>
    /// 收款用户OpenID
    /// </summary>
    [JsonPropertyName("openid")]
    public string OpenId { get; set; } = string.Empty;

    /// <summary>
    /// 收款用户姓名（加密，金额≥2000元必传）
    /// </summary>
    [JsonPropertyName("user_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UserName { get; set; }

    /// <summary>
    /// 转账金额（单位：分）
    /// </summary>
    [JsonPropertyName("transfer_amount")]
    public int TransferAmount { get; set; }

    /// <summary>
    /// 转账备注（32字符内）
    /// </summary>
    [JsonPropertyName("transfer_remark")]
    public string TransferRemark { get; set; } = string.Empty;

    /// <summary>
    /// 通知地址
    /// </summary>
    [JsonPropertyName("notify_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NotifyUrl { get; set; }

    /// <summary>
    /// 用户收款感知
    /// </summary>
    [JsonPropertyName("user_recv_perception")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UserRecvPerception { get; set; }

    /// <summary>
    /// 转账场景报备信息
    /// </summary>
    [JsonPropertyName("transfer_scene_report_infos")]
    public List<WeChatPayTransferSceneReportInfo> TransferSceneReportInfos { get; set; } = [];
}
