namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

/// <summary>
/// 订单业务支付方式
/// </summary>
public sealed record class OrderBusinessPaymentMethod
{
    public PaymentMethod PaymentMethod { get; set; }

    public PaymentType PaymentType { get; set; }
}

/// <summary>
/// 订单业务发起支付方式，例如同一个支付平台的扫码支付、移动端页面支付、PC网站支付等
/// </summary>
public sealed record class OrderBusinessLaunchPaymentMethod
{
    public PaymentMethod PaymentMethod { get; set; }

    public WebPaymentType PaymentType { get; set; }
}