using System.ComponentModel.DataAnnotations;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

public sealed partial class AddRefundBillModel
{
    /// <summary>
    /// 退款单号
    /// </summary>
    [StringLength(64)]
    public string RefundNumber { get; set; } = string.Empty;

    /// <summary>
    /// 退款金额
    /// </summary>
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// 退款方式类型
    /// </summary>
    public RefundChannelType RefundMethodType { get; set; }

    /// <summary>
    /// 第三方平台交易流水号
    /// </summary>
    [StringLength(64)]
    public string? ThirdPartyPlatformNumber { get; set; }

    /// <summary>
    /// 退款状态
    /// </summary>
    public RefundStatus RefundStatus { get; set; }

    /// <summary>
    /// 退款失败原因
    /// </summary>
    [StringLength(2000)]
    public string? RefundFailureReason { get; set; }
}
