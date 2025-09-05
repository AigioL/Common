using System.ComponentModel;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

/// <summary>
/// 支付类型
/// </summary>
public enum PaymentType : byte
{
    /// <summary>
    /// 支付宝
    /// </summary>
    [Description("支付宝")]
    Alipay = 1,

    /// <summary>
    /// 微信支付
    /// </summary>
    [Description("微信支付")]
    WeChatPay = 2,

    /// <summary>
    /// 云闪付支付
    /// </summary>
    [Description("云闪付支付")]
    UnionPay = 3,
}
