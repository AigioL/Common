namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

public enum TradeStatus
{
    /// <summary>
    /// 交易创建，等待买家付款
    /// </summary>
    WaitBuyerPay,

    /// <summary>
    /// 未付款交易超时关闭，或支付完成后全额退款
    /// </summary>
    TradeClosed,

    /// <summary>
    /// 交易支付成功
    /// </summary>
    TradeSuccess,

    /// <summary>
    /// 交易结束，不可退款
    /// </summary>
    TradeFinished,
}