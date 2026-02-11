using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.Statistics;

public sealed partial class OrderAmountQtyTableModel
{
    /// <summary>
    /// 订单类型
    /// </summary>
    public OrderType OrderType { get; set; }

    /// <summary>
    /// 订单业务类型
    /// </summary>
    public int OrderBusinessTypeId { get; set; }

    /// <summary>
    /// 商品类型
    /// </summary>
    public string? GoodsType { get; set; }

    /// <summary>
    /// 销售数量
    /// </summary>
    public int PaymentCount { get; set; }

    /// <summary>
    /// 退款数量
    /// </summary>
    public int RefundCount { get; set; }

    /// <summary>
    /// 销售金额
    /// </summary>
    public decimal PaymentAmount { get; set; }

    /// <summary>
    /// 实际到账金额
    /// </summary>
    public decimal ActualAmount { get; set; }

    /// <summary>
    /// 退款金额
    /// </summary>
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// 利润
    /// </summary>
    public decimal Profit { get; set; }

    /// <summary>
    /// 分成
    /// </summary>
    public decimal Share { get; set; }

    /// <summary>
    /// 上个月退款数量
    /// </summary>
    public int LastMonthRefundCount { get; set; }

    /// <summary>
    /// 上个月退款金额
    /// </summary>
    public decimal LastMonthRefundAmount { get; set; }
}
