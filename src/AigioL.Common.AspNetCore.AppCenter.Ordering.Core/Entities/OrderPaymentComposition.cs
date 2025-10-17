using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;

/// <summary>
/// 订单支付组成
/// </summary>
[Table("OrderPaymentCompositions")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class OrderPaymentComposition :
    TenantBaseEntity<Guid>,
    INEWSEQUENTIALID,
    INote
{
    /// <summary>
    /// 订单 Id
    /// </summary>
    [Required]
    [StringLength(MaxLengths.OrderId)]
    [Comment("订单 Id")]
    public required string OrderId { get; set; }

    public virtual Order Order { get; set; } = null!;

    /// <summary>
    /// 支付单号
    /// </summary>
    [Required]
    [Comment("支付单号")]
    public required string PaymentNumber { get; set; }

    /// <summary>
    /// 支付方式
    /// </summary>
    [Comment("支付方式")]
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// 支付类型
    /// </summary>
    [Comment("支付类型")]
    public PaymentType PaymentType { get; set; }

    /// <summary>
    /// 支付状态
    /// </summary>
    [Comment("支付状态")]
    public PaymentStatus PaymentStatus { get; set; }

    /// <summary>
    /// 支付时间
    /// </summary>
    [Comment("支付时间")]
    public DateTimeOffset? PaymentTime { get; set; }

    /// <summary>
    /// 用户优惠劵信息 Id
    /// </summary>
    [Comment("用户优惠劵信息 Id")]
    public Guid? UserCouponInfoId { get; set; }

    /// <summary>
    /// 用户优惠劵信息
    /// </summary>
    public virtual UserCouponInfo UserCouponInfo { get; set; } = null!;

    /// <summary>
    /// 支付金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("支付金额")]
    public decimal PaymentAmount { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<OrderPaymentComposition>
    {
        public sealed override void Configure(EntityTypeBuilder<OrderPaymentComposition> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.UserCouponInfo)
                .WithMany(x => x.OrderPaymentCompositions)
                .HasForeignKey(x => x.UserCouponInfoId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}