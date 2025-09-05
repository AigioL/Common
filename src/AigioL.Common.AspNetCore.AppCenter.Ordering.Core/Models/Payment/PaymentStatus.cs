using System.ComponentModel;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

/// <summary>
/// 支付状态
/// </summary>
public enum PaymentStatus : byte
{
    /// <summary>
    /// 待付款
    /// </summary>
    [Description("待付款")]
    WaitPay = 0,

    /// <summary>
    /// 已付款
    /// </summary>
    [Description("已付款")]
    Paid = 1,

    /// <summary>
    /// 已退款
    /// </summary>
    [Description("已退款")]
    Refunded = 2,
}