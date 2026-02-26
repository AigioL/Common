using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;

/// <summary>
/// 会员产品密钥
/// </summary>
[Table(nameof(MembershipProductKeyRecord) + "s")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class MembershipProductKeyRecord :
    OperatorBaseEntity<Guid>,
    INEWSEQUENTIALID,
    IDisable
{
    public virtual MembershipGoods MembershipGoods { get; set; } = null!;

    /// <summary>
    /// 会员商品 Id
    /// </summary>
    [Required]
    [Comment("会员商品 Id")]
    public Guid MembershipGoodsId { get; set; }

    /// <summary>
    /// 充值天数
    /// </summary>
    [Required]
    [Comment("充值天数")]
    public int RechargeDays { get; set; }

    /// <summary>
    /// 是否已使用
    /// </summary>
    [Required]
    [Comment("是否已使用")]
    public bool IsUsed { get; set; }

    [Comment("使用时间")]
    public DateTimeOffset? UsageTime { get; set; }

    /// <inheritdoc/>
    [Comment("是否禁用")]
    public bool Disable { get; set; }

    /// <summary>
    /// 分成 KOL 用户 Id
    /// </summary>
    [Comment("分成 KOL 用户 Id")]
    public Guid? RevenueShareRecipientKolUserId { get; set; }

    /// <summary>
    /// 分成比例
    /// </summary>
    [Comment("分成比例")]
    [Precision(10, 8)]
    public decimal RevenueSharePercentage { get; set; }

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<MembershipProductKeyRecord>
    {
        public override void Configure(EntityTypeBuilder<MembershipProductKeyRecord> builder)
        {
            base.Configure(builder);

            builder
                .HasOne(x => x.MembershipGoods)
                .WithMany()
                .HasForeignKey(x => x.MembershipGoodsId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
