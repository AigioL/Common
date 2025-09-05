using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Summaries;

/// <summary>
/// 订单的金额数量汇总表实体类
/// </summary>
[Table("OrderAmountQtySummaries")]
public partial class OrderAmountQtySummary :
    Entity<Guid>,
    ICreationTime,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 当天订单金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("当天订单金额")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 当天订单退款金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("当天订单退款金额")]
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// 当天订单数量
    /// </summary>
    [Comment("当天订单数量")]
    public int Quantity { get; set; }

    /// <summary>
    /// 当天订单退款数量
    /// </summary>
    [Comment("当天订单退款数量")]
    public int RefundQuantity { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    [Comment("业务类型")]
    public int BusinessTypeId { get; set; }

    /// <summary>
    /// 商品类型（订单备注字段）
    /// </summary>
    [Comment("商品类型")]
    public required string GoodsType { get; set; }

    /// <summary>
    /// 支付类型
    /// </summary>
    [Comment("支付类型")]
    public PaymentType PaymentType { get; set; }

    /// <summary>
    /// 统计日期（当天的数据）
    /// </summary>
    [Comment("统计日期（当天的数据）")]
    public DateTimeOffset StatisticsTime { get; set; }

    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }
}