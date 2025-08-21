using AigioL.Common.AspNetCore.AppCenter.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;

/// <summary>
/// 会员商品与用户首次购买记录多对多关系表实体类
/// </summary>
[Table(nameof(MembershipGoodUserFirstRecord) + "s")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class MembershipGoodUserFirstRecord
{
    public Guid MembershipGoodsId { get; set; }

    public Guid UserId { get; set; }

    public Guid MembershipBusinessOrderId { get; set; }

    public virtual MembershipGood MembershipGoods { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual MembershipBusinessOrder MembershipBusinessOrder { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<MembershipGoodUserFirstRecord>
    {
        public void Configure(EntityTypeBuilder<MembershipGoodUserFirstRecord> builder)
        {
            builder.HasIndex(x => new { x.UserId, x.MembershipGoodsId, x.MembershipBusinessOrderId });

            builder.HasOne(x => x.MembershipBusinessOrder)
                .WithOne()
                .HasForeignKey<MembershipGoodUserFirstRecord>(x => x.MembershipBusinessOrderId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}