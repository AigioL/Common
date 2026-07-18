using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.PartnerCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.PartnerCenter.Entities;

/// <summary>
/// PC 用户钱包变更记录
/// </summary>
[Table("PCUserWalletChangeRecords")]
[Index(nameof(UserId))]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class PCUserWalletChangeRecord :
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
    /// 钱包值类型
    /// </summary>
    [Comment("钱包值类型")]
    public PCUserWalletValueType Type { get; set; }

    /// <summary>
    /// 事件
    /// </summary>
    [Comment("事件")]
    public PCUserWalletValueEvent Event { get; set; }

    /// <summary>
    /// 支付方向
    /// </summary>
    [Comment("支付方向")]
    public UserWalletPaymentDirection Direction { get; set; }

    /// <summary>
    /// 变更值
    /// </summary>
    [Precision(18, 4)]
    [Comment("变更值")]
    public decimal ChangeValue { get; set; }

    /// <summary>
    /// 结果值
    /// </summary>
    [Precision(18, 4)]
    [Comment("结果值")]
    public decimal ResultValue { get; set; }

    /// <summary>
    /// 变更原因
    /// </summary>
    [StringLength(MaxLengths.ChangeReason)]
    [Comment("变更原因")]
    public string? Reason { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    /// <summary>
    /// 变更时间
    /// </summary>
    [Comment("变更时间")]
    public DateTimeOffset CreateTime { get; set; }

    /// <summary>
    /// 来源 Id（用于幂等性保证）
    /// </summary>
    [Comment("来源 Id")]
    [StringLength(50)]
    public string? SourceId { get; set; }

    public virtual PCUserWallet Wallet { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<PCUserWalletChangeRecord>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<PCUserWalletChangeRecord> builder)
        {
            builder.HasIndex(x => x.SourceId).IsUnique();
        }
    }
}
