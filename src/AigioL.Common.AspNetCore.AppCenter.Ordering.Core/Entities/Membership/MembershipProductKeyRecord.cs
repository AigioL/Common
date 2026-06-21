using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
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
    IDisable,
    INote
{
    public virtual MembershipGoods MembershipGoods { get; set; } = null!;

    /// <summary>
    /// 会员商品 Id
    /// </summary>
    [Comment("会员商品 Id")]
    public Guid MembershipGoodsId { get; set; }

    /// <summary>
    /// 充值天数
    /// </summary>
    [Comment("充值天数")]
    [Obsolete("use RechargeTimeSpan")]
    public int RechargeDays { get; set; }

    /// <summary>
    /// 充值时间跨度
    /// </summary>
    [Comment("充值时间跨度")]
    public TimeSpan RechargeTimeSpan { get; set; }

    /// <summary>
    /// 按量付费的会员时长
    /// </summary>
    [Comment("按量付费的会员时长")]
    public TimeSpan PayAsYoGo { get; set; }

    /// <summary>
    /// 是否已使用
    /// </summary>
    [Comment("是否已使用")]
    public bool IsUsed { get; set; }

    [Comment("使用时间")]
    public DateTimeOffset? UsageTime { get; set; }

    /// <inheritdoc/>
    [Comment("是否禁用")]
    public bool Disable { get; set; }

    /// <inheritdoc/>
    [StringLength(MaxLengths.Text)]
    public string? Note { get; set; }

    /// <summary>
    /// 合作伙伴用户 Id
    /// </summary>
    [Comment("合作伙伴用户 Id")]
    public Guid? PCUserId { get; set; }

    /// <summary>
    /// 合作伙伴用户
    /// </summary>
    public virtual PCUser? PCUser { get; set; }

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

            builder.HasOne(x => x.PCUser)
                .WithMany(x => x.MembershipProductKeyRecords)
                .HasForeignKey(x => x.PCUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
