using System.ComponentModel;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;

/// <summary>
/// 商品充值状态
/// </summary>
public enum GoodRechargeStatus : byte
{
    /// <summary>
    /// 待充值，适用于订单支付完毕后
    /// </summary>
    [Description("待充值")]
    Pending = 1,

    /// <summary>
    /// 已充值
    /// </summary>
    [Description("已充值")]
    Recharged = 2,

    /// <summary>
    /// 充值进行中
    /// </summary>
    [Description("充值进行中")]
    InProgress = 3,

    /// <summary>
    /// 充值异常
    /// </summary>
    [Description("充值异常")]
    Exception = 4,

    /// <summary>
    /// 充值关闭，适用于订单还未充值其他原因终止
    /// </summary>
    [Description("充值关闭")]
    Closed = 5,

    /// <summary>
    /// 等待支付
    /// </summary>
    [Description("等待支付")]
    Waiting = 6,

    /// <summary>
    /// 充值撤回，适用于订单充值成功后用户退款
    /// </summary>
    [Description("充值撤回")]
    RechargeReturn = 7
}
