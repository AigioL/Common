using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.Statistics;

/// <summary>
/// 统计订单的金额数量响应
/// </summary>
public sealed partial class StatisticsOrderAmountQtyModel
{
    /// <summary>
    /// 订单金额
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 订单数量
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// 订单退款金额
    /// </summary>
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// 订单退款数量
    /// </summary>
    public decimal RefundQuantity { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    public int BusinessTypeId { get; set; }

    /// <summary>
    /// 商品类型（订单备注字段）
    /// </summary>
    public string GoodsType { get; set; } = string.Empty;

    /// <summary>
    /// 支付类型
    /// </summary>
    public PaymentType PaymentType { get; set; }

    /// <summary>
    /// 统计日期（当天的数据）
    /// </summary>
    public DateTimeOffset StatisticsTime { get; set; }
}