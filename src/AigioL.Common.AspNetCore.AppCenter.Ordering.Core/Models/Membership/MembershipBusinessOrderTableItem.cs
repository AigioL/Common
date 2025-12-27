using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;

public sealed partial class MembershipBusinessOrderTableItem : IReadOnlyId<Guid>, ICreateTime
{
    public Guid Id { get; set; }

    /// <summary>
    /// 用户 Id
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 会员商品 Id
    /// </summary>
    public Guid MembershipGoodsId { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    public required string GoodsName { get; set; }

    /// <summary>
    /// 商品编号
    /// </summary>
    public required string GoodsNo { get; set; }

    /// <summary>
    /// 会员订阅类型
    /// </summary>
    public MembershipLicenseFlags MemberLicenseType { get; set; }

    /// <summary>
    /// 充值天数
    /// </summary>
    public int RechargeDays { get; set; }

    /// <summary>
    /// 通用订单ID
    /// </summary>
    public Guid? GenericOrderId { get; set; }

    /// <summary>
    /// 应收金额
    /// </summary>
    public decimal AmountReceivable { get; set; }

    /// <summary>
    /// 实收金额
    /// </summary>
    public decimal AmountReceived { get; set; }

    /// <summary>
    /// 支付状态
    /// </summary>
    public OrderStatus PaymentStatus { get; set; }

    /// <summary>
    /// 充值状态
    /// </summary>
    public GoodsRechargeStatus GoodsRechargeStatus { get; set; }

    /// <summary>
    /// 支付时间
    /// </summary>
    public DateTimeOffset? PaymentTime { get; set; }

    /// <summary>
    /// 充值完成时间
    /// </summary>
    public DateTimeOffset? RechargeCompletionTime { get; set; }

    /// <summary>
    /// 业务来源
    /// </summary>
    public MembershipBusinessSource BusinessSource { get; set; }

    /// <summary>
    /// 产品密钥记录ID
    /// </summary>
    public Guid? ProductKeyRecordId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreateTime { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Note { get; set; }

    public string? Cdkey { get; set; }
}
