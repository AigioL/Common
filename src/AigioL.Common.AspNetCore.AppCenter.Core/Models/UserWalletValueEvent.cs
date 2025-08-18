using System.ComponentModel;

namespace AigioL.Common.AspNetCore.AppCenter.Models;

/// <summary>
/// 用户钱包值事件
/// 可对用户钱包值的变动事件
/// </summary>
public enum UserWalletValueEvent : byte
{
    #region 增加

    /// <summary>
    /// 充值
    /// </summary>
    [Description("充值")]
    Recharge = 1,

    /// <summary>
    /// 退款
    /// </summary>
    [Description("退款")]
    Refund,

    /// <summary>
    /// 核心商城退款
    /// </summary>
    [Description("核心商城退款")]
    CoreShopRefund,

    #endregion 增加

    #region 减少

    /// <summary>
    /// 消费
    /// </summary>
    [Description("消费")]
    Consumption = 21,

    /// <summary>
    /// 提现
    /// </summary>
    [Description("提现")]
    Withdrawal,

    /// <summary>
    /// 核心商城购物
    /// </summary>
    [Description("核心商城购物")]
    CoreShopPay,

    #endregion 减少

    #region 变动

    /// <summary>
    /// 调整
    /// </summary>
    [Description("调整")]
    Adjust = 41,

    #endregion 变动
}