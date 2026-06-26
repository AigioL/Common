using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.PartnerCenter.Entities;

/// <summary>
/// PC 用户钱包
/// </summary>
[Table("PCUserWallets")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class PCUserWallet :
    IEntity<Guid>,
    IUpdateTime,
    IRowVersion
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Comment("用户 Id")]
    public Guid Id { get; set; }

    /// <summary>
    /// 关联的 PC 用户
    /// </summary>
    public virtual PCUser PCUser { get; set; } = null!;

    /// <summary>
    /// 可提现金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("可提现金额")]
    public decimal WithdrawableAmount { get; set; }

    /// <summary>
    /// 已提现金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("已提现金额")]
    public decimal WithdrawnAmount { get; set; }

    /// <summary>
    /// 累计结算金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("累计结算金额")]
    public decimal CumulativeSettlementAmount { get; set; }

    /// <inheritdoc/>
    [Comment("修改时间")]
    public DateTimeOffset UpdateTime { get; set; }

    /// <inheritdoc/>
    [Comment("并发令牌")]
    public uint RowVersion { get; set; }

    /// <summary>
    /// 钱包变更记录
    /// </summary>
    public virtual List<PCUserWalletChangeRecord> WalletChangeRecords { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<PCUserWallet>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<PCUserWallet> builder)
        {
            builder.HasOne(u => u.PCUser)
                .WithOne()
                .HasForeignKey<PCUserWallet>(u => u.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.WalletChangeRecords)
                .WithOne()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
