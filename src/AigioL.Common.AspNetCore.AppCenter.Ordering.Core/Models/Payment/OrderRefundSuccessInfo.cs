namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

/// <summary>
/// 订单退款成功信息
/// </summary>
/// <param name="OrderNumber">订单号</param>
/// <param name="RefundNumber">退款单号</param>
/// <param name="PaymentPlatform">支付平台</param>
public sealed record OrderRefundSuccessInfo(string OrderNumber, string RefundNumber, PaymentType PaymentPlatform);