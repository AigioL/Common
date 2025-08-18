using System.ComponentModel;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

/// <summary>
/// 订单状态
/// </summary>
public enum OrderStatus : byte
{
    /// <summary>
    /// 待付款
    /// </summary>
    [Description("待付款")]
    WaitPay = 1,

    /// <summary>
    /// 已付款
    /// </summary>
    [Description("已付款")]
    Paid,

    /// <summary>
    /// 已过期
    /// </summary>
    [Description("已过期")]
    Expired,

    /// <summary>
    /// 已取消
    /// </summary>
    [Description("已取消")]
    Canceled,

    /// <summary>
    /// 已关闭
    /// </summary>
    [Description("已关闭")]
    Closed,

    /// <summary>
    /// 已完成
    /// </summary>
    [Description("已完成")]
    Completed,

    /// <summary>
    /// 已退款
    /// </summary>
    [Description("已退款")]
    Refunded,
}