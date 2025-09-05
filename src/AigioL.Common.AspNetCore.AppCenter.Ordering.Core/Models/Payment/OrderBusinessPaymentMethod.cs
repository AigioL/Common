namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

/// <summary>
/// 订单业务支付方式
/// </summary>
public sealed record class OrderBusinessPaymentMethod
{
    public PaymentMethod PaymentMethod { get; set; }

    public PaymentType PaymentType { get; set; }
}