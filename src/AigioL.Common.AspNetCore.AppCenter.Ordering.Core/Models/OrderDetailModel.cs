using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

public sealed partial record class OrderDetailModel
{
    /// <summary>
    /// 订单主键
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    public string OrderNumber { get => Id; set => Id = value; } // 兼容旧数据结构

    /// <summary>
    /// 订单类型
    /// </summary>
    public OrderType Type { get; set; }

    /// <summary>
    /// 订单来源终端
    /// </summary>
    public DevicePlatform2 Source { get; set; }

    /// <summary>
    /// 订单超时时间
    /// </summary>
    public DateTimeOffset Timeout { get; set; }

    /// <summary>
    /// 订单状态
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// 用户 Id
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 应收金额
    /// </summary>
    public decimal AmountReceivable { get; set; }

    /// <summary>
    /// 实收金额
    /// </summary>
    public decimal AmountReceived { get; set; }

    /// <summary>
    /// 支付时间
    /// </summary>
    public DateTimeOffset? PaymentTime { get; set; }

    /// <summary>
    /// 业务类型，关联的支付业务类型枚举
    /// </summary>
    public int BusinessType { get; set; }

    /// <summary>
    /// 业务 Id，业务类型订单的 Id
    /// </summary>
    public Guid BusinessId { get; set; }

    /// <summary>
    /// 订单备注
    /// </summary>
    public string? Note { get; set; }

    /// <inheritdoc cref="Note"/> 
    public string? Remarks { get => Note; set => Note = value; } // 兼容旧数据结构

    ///// <summary>
    ///// 订单支付组成
    ///// </summary>
    //public List<OrderPaymentCompositionInfoModel>? PaymentCompositions { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreationTime { get; set; }

    ///// <summary>
    ///// 业务订单信息
    ///// </summary>
    //public BizOrderInfoDTO? BusinessOrderInfo { get; set; }

    ///// <summary>
    ///// 售后信息
    ///// </summary>
    //public List<AftersalesBillItemDTO>? AftersalesBills { get; set; }
}
