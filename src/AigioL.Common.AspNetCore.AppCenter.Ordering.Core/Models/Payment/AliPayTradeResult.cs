namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

/// <summary>
/// 支付宝交易结果
/// </summary>
public sealed record AliPayTradeResult(string OutTradeNo, string TradeNo, string BuyerUserId, string BuyerLogonId,
    decimal BuyerPayAmount, decimal TotalAmount, decimal ReceiptAmount, DateTimeOffset SendPayDate, TradeStatus TradeStatus);