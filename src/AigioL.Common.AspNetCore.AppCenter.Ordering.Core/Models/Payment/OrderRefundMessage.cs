namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

/// <summary>
/// 订单退款消息内容
/// </summary>
/// <param name="OrderNumber">订单号</param>
/// <param name="RefundNumber">退款单号</param>
public sealed record OrderRefundMessage(string OrderNumber, string RefundNumber);