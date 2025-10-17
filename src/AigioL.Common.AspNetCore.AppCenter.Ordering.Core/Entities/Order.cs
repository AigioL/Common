using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;

/// <summary>
/// 通用订单表实体类
/// </summary>
[Table("Orders")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
[Index(nameof(Source)), Index(nameof(Status)), Index(nameof(BusinessTypeId))]
public partial class Order :
    TenantBaseEntity<string>,
    INote
{
    /// <summary>
    /// 订单号
    /// </summary>
    [StringLength(MaxLengths.OrderId)]
    [Comment("订单号")]
    public required override string Id { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    [NotMapped]
    public string OrderNumber { get => Id; set => Id = value; }

    /// <summary>
    /// 订单类型
    /// </summary>
    [Comment("订单类型")]
    public OrderType Type { get; set; }

    /// <summary>
    /// 订单来源终端
    /// </summary>
    [Comment("订单来源终端")]
    public DevicePlatform2 Source { get; set; }

    /// <summary>
    /// 订单超时时间
    /// </summary>
    [Comment("订单超时时间")]
    public DateTimeOffset Timeout { get; set; }

    /// <summary>
    /// 订单状态
    /// </summary>
    [Comment("订单状态")]
    public OrderStatus Status { get; set; }

    /// <summary>
    /// 用户 Id
    /// </summary>
    [Comment("用户 Id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 应收金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("应收金额")]
    public decimal AmountReceivable { get; set; }

    /// <summary>
    /// 实收金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("实收金额")]
    public decimal AmountReceived { get; set; }

    /// <summary>
    /// 支付时间
    /// </summary>
    [Comment("支付时间")]
    public DateTimeOffset? PaymentTime { get; set; }

    /// <summary>
    /// 支付类型
    /// </summary>
    [Comment("支付类型")]
    public PaymentType? PaymentType { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    [Comment("业务类型")]
    public int BusinessTypeId { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    /// <summary>
    /// 商家扣款协议 Id
    /// </summary>
    [Comment("商家扣款协议 Id")]
    public Guid? MerchantDeductionAgreementId { get; set; }

    public virtual MerchantDeductionAgreement MerchantDeductionAgreement { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    /// <summary>
    /// 订单支付组成
    /// </summary>
    public virtual List<OrderPaymentComposition> PaymentCompositions { get; set; } = null!;

    /// <summary>
    /// 售后单
    /// </summary>
    public virtual List<AftersalesBill> AftersalesBills { get; set; } = null!;

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<Order>
    {
        public sealed override void Configure(EntityTypeBuilder<Order> builder)
        {
            base.Configure(builder);

            //builder.HasAlternateKey(o => o.OrderNumber);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.PaymentCompositions)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
