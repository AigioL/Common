using System.ComponentModel;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

/// <summary>
/// 支付方式
/// </summary>
public enum PaymentMethod : byte
{
    /// <summary>
    /// 现金
    /// </summary>
    [Description("现金")]
    Cash = 1,

    /// <summary>
    /// 在线支付
    /// </summary>
    [Description("在线支付")]
    Online = 2,

    /// <summary>
    /// 余额
    /// </summary>
    [Description("余额")]
    Balance = 3,

    /// <summary>
    /// 优惠卷
    /// </summary>
    [Description("优惠卷")]
    Coupons = 4,

    /// <summary>
    /// 银行转账
    /// </summary>
    [Description("银行转账")]
    BankTransfer = 5,

    /// <summary>
    /// POS 机
    /// </summary>
    [Description("POS机")]
    PointOfSalesTerminal = 6,
}