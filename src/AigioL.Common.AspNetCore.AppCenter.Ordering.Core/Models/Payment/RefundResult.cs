namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

/// <summary>
/// 退款结果
/// </summary>
/// <param name="OutRefundNo">商家退款号</param>
/// <param name="OutTradeNo">商家交易号</param>
/// <param name="RefundNo">退款号</param>
/// <param name="TotalAmount">交易金额</param>
/// <param name="RefundAmount">退款金额</param>
public sealed record RefundResult(string OutRefundNo, string OutTradeNo, string RefundNo, decimal TotalAmount, decimal RefundAmount);