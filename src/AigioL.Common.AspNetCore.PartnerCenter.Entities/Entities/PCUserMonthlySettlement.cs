using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace AigioL.Common.AspNetCore.PartnerCenter.Entities;

/// <summary>
/// PC 用户月度结算记录
/// </summary>
[Table("PCUserMonthlySettlements")]
[Index(nameof(UserId))]
[Index(nameof(SettlementYearMonth))]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class PCUserMonthlySettlement :
    Entity<Guid>,
    INEWSEQUENTIALID,
    INote,
    ICreateTime
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    [Comment("用户 Id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 结算年月（格式：yyyy-MM）
    /// </summary>
    [Required]
    [StringLength(7)]
    [Comment("结算年月")]
    public required string SettlementYearMonth { get; set; }

    /// <summary>
    /// 结算金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("结算金额")]
    public decimal SettlementAmount { get; set; }

    /// <summary>
    /// 结算状态
    /// </summary>
    [Comment("结算状态")]
    public PCUserSettlementStatus Status { get; set; }

    /// <summary>
    /// 结算完成时间
    /// </summary>
    [Comment("结算完成时间")]
    public DateTimeOffset? SettlementTime { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Comment("创建时间")]
    public DateTimeOffset CreateTime { get; set; }

    public virtual PCUserWallet Wallet { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<PCUserMonthlySettlement>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<PCUserMonthlySettlement> builder)
        {
            builder.HasOne(u => u.Wallet)
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
