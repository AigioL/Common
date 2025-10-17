using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;

/// <summary>
/// 优惠券表实体类
/// </summary>
[Table("Coupons")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class Coupon :
    TenantBaseEntity<Guid>,
    INEWSEQUENTIALID,
    IDescription,
    ICouponEntity
{
    [Required]
    [Comment("优惠劵名称")]
    [StringLength(MaxLengths.CouponName)]
    public required string Name { get; set; }

    [Comment("优惠劵类型")]
    public CouponType CouponType { get; set; }

    #region Effective 有效期

    [Comment("有效开始时间")]
    public DateTimeOffset EffectiveStartTime { get; set; }

    [Comment("有效结束时间")]
    public DateTimeOffset EffectiveEndTime { get; set; }

    #endregion

    #region Restricted 限定

    [Comment("限定费用类型 Id")]
    public Guid? RestrictedFeeTypeId { get; set; }

    /// <summary>
    /// 限定费用类型
    /// </summary>
    public virtual FeeType RestrictedFeeType { get; set; } = null!;

    /// <summary>
    /// 限定费用类型名称
    /// </summary>
    [Comment("限定费用类型名称")]
    [StringLength(MaxLengths.CouponName)]
    public string? RestrictedFeeTypeName { get; set; }

    /// <summary>
    /// 限定用户类型
    /// </summary>
    [Comment("限定用户类型")]
    public CouponRestrictedUserType RestrictedUserType { get; set; }

    /// <summary>
    /// 限定可用金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("限定可用金额")]
    public decimal RestrictedAmountAvailable { get; set; }

    #endregion

    /// <summary>
    /// 劵面值
    /// </summary>
    [Precision(18, 4)]
    [Comment("劵面值")]
    public decimal Value { get; set; }

    /// <summary>
    /// 实际购买值
    /// </summary>
    [Precision(18, 4)]
    [Comment("实际购买值")]
    public decimal ActualPurchaseValue { get; set; }

    #region Release 发行

    [Comment("发行场景")]
    public CouponReleaseScenario ReleaseScenario { get; set; }

    /// <summary>
    /// 发行数量
    /// </summary>
    [Comment("发行数量")]
    public int ReleaseNumber { get; set; }

    /// <summary>
    /// 发行时间
    /// </summary>
    [Comment("发行时间")]
    public DateTimeOffset ReleaseTime { get; set; }

    #endregion

    [Comment("描述")]
    public string? Description { get; set; }

    public virtual List<UserCouponInfo> UserCouponInfos { get; set; } = null!;

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<Coupon>
    {
        public sealed override void Configure(EntityTypeBuilder<Coupon> builder)
        {
            base.Configure(builder);

            builder
                .HasMany(x => x.UserCouponInfos)
                .WithOne(x => x.Coupon)
                .HasForeignKey(x => x.CouponId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.CouponType);
            builder.HasIndex(x => x.ReleaseScenario);
            builder.HasIndex(x => x.RestrictedUserType);
        }
    }
}