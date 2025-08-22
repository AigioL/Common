using AigioL.Common.AspNetCore.AppCenter.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;

/// <summary>
/// 会员商品与用户首次购买记录多对多关系表实体类
/// </summary>
[Table(nameof(MembershipGoodsUserFirstRecord) + "s")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class MembershipGoodsUserFirstRecord
{
    public Guid MembershipGoodsId { get; set; }

    public Guid UserId { get; set; }

    public Guid MembershipBusinessOrderId { get; set; }

    public virtual MembershipGoods MembershipGoods { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual MembershipBusinessOrder MembershipBusinessOrder { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<MembershipGoodsUserFirstRecord>
    {
        public void Configure(EntityTypeBuilder<MembershipGoodsUserFirstRecord> builder)
        {
            builder.HasIndex(x => new { x.UserId, x.MembershipGoodsId, x.MembershipBusinessOrderId });

            builder.HasOne(x => x.MembershipBusinessOrder)
                .WithOne()
                .HasForeignKey<MembershipGoodsUserFirstRecord>(x => x.MembershipBusinessOrderId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}