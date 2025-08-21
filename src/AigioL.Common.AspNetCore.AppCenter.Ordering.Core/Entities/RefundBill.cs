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
/// 退款单表实体类
/// </summary>
[Table("RefundBills")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class RefundBill :
    OperatorBaseEntity<Guid>,
    ITenantId,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 售后单 Id
    /// </summary>
    [Comment("售后单 Id")]
    public Guid AftersalesBillId { get; set; }

    /// <summary>
    /// 用户 Id
    /// </summary>
    [Comment("用户 Id")]
    public Guid UserId { get; set; }

    /// <inheritdoc/>
    [Comment("租户 Id")]
    public Guid TenantId { get; set; }

    /// <summary>
    /// 退款单号
    /// </summary>
    [StringLength(64)]
    [Required]
    [Comment("退款单号")]
    public required string RefundNumber { get; set; }

    /// <summary>
    /// 退款金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("退款金额")]
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// 退款方式类型
    /// </summary>
    [Comment("退款方式类型")]
    public RefundChannelType RefundMethodType { get; set; }

    /// <summary>
    /// 第三方平台交易流水号
    /// </summary>
    [StringLength(64)]
    [Comment("第三方平台交易流水号")]
    public string? ThirdPartyPlatformNumber { get; set; }

    /// <summary>
    /// 退款状态
    /// </summary>
    [Comment("退款状态")]
    public RefundStatus RefundStatus { get; set; }

    /// <summary>
    /// 退款完成时间
    /// </summary>
    [Comment("退款完成时间")]
    public DateTimeOffset? RefundFinishTime { get; set; }

    /// <summary>
    /// 退款失败原因
    /// </summary>
    [Comment("退款失败原因")]
    public string? RefundFailureReason { get; set; }

    public virtual AftersalesBill? AftersalesBill { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<RefundBill>
    {
        public sealed override void Configure(EntityTypeBuilder<RefundBill> builder)
        {
            base.Configure(builder);

            builder.HasAlternateKey(o => o.RefundNumber);

            builder.HasOne(p => p.AftersalesBill)
                .WithOne(d => d.RefundBill)
                .HasForeignKey<RefundBill>(p => p.AftersalesBillId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}