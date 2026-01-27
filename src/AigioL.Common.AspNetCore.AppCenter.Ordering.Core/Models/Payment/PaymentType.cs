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

/// <summary>
/// 发起支付类型，例如同一个支付平台的扫码支付、移动端页面支付、PC网站支付等
/// </summary>
public enum WebPaymentType
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

    /// <summary>
    /// 微信支付（扫码支付）
    /// </summary>
    [Description("微信支付")]
    WeChatPayNative = 4,

    /// <summary>
    /// 支付宝（移动端页面）
    /// </summary>
    [Description("支付宝")]
    AlipayMWEB = 5,
}

public static partial class WebPaymentTypeExtensions
{
    public static PaymentType ToPaymentType(this WebPaymentType webPaymentType) =>
        webPaymentType switch
        {
            WebPaymentType.Alipay => PaymentType.Alipay,
            WebPaymentType.WeChatPay => PaymentType.WeChatPay,
            WebPaymentType.UnionPay => PaymentType.UnionPay,
            WebPaymentType.WeChatPayNative => PaymentType.WeChatPay,
            WebPaymentType.AlipayMWEB => PaymentType.Alipay,
            _ => throw new ArgumentOutOfRangeException(nameof(webPaymentType), $"不支持的发起支付类型：{webPaymentType}"),
        };
}