using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;

/// <summary>
/// 售后单表实体类
/// </summary>
[Table("AftersalesBills")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class AftersalesBill :
    OperatorBaseEntity<Guid>,
    ITenantId,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 订单 Id
    /// </summary>
    [Required]
    [StringLength(MaxLengths.OrderId)]
    [Comment("订单 Id")]
    public required string OrderId { get; set; }

    /// <summary>
    /// 用户 Id
    /// </summary>
    [Comment("用户 Id")]
    public Guid UserId { get; set; }

    /// <inheritdoc/>
    [Comment("租户 Id")]
    public Guid TenantId { get; set; }

    /// <summary>
    /// 售后单号
    /// </summary>
    [StringLength(64)]
    [Required]
    [Comment("售后单号")]
    public required string AftersalesNumber { get; set; }

    /// <summary>
    /// 退款金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("退款金额")]
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// 审核状态
    /// </summary>
    [Comment("审核状态")]
    public AuditStatus AuditStatus { get; set; }

    /// <summary>
    /// 退款原因
    /// </summary>
    [StringLength(500)]
    [Comment("退款原因")]
    public string? RefundReason { get; set; }

    /// <summary>
    /// 卖家备注
    /// </summary>
    [StringLength(2000)]
    [Comment("卖家备注")]
    public string? SellerNote { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual RefundBill RefundBill { get; set; } = null!;

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<AftersalesBill>
    {
        public sealed override void Configure(EntityTypeBuilder<AftersalesBill> builder)
        {
            base.Configure(builder);

            builder.HasAlternateKey(o => o.AftersalesNumber);

            builder.HasOne(u => u.Order)
                .WithMany(o => o.AftersalesBills)
                .HasForeignKey(u => u.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(u => u.User)
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}