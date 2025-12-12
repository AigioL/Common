namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

/// <summary>
/// 微信支付交易结果
/// </summary>
public sealed partial record WechatPayTradeResult(string OpenId, string OutTradeNo, DateTimeOffset TimeEnd, decimal TotalAmount, string TransactionId, TradeStatus TradeStatus);