using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;

/// <summary>
/// 用户优惠券信息表实体类
/// </summary>
[Table("UserCouponInfos")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class UserCouponInfo :
    TenantBaseEntity<Guid>,
    INEWSEQUENTIALID,
    ICouponEntity
{
    /// <summary>
    /// 设置优惠劵信息
    /// </summary>
    /// <param name="value"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCoupon(Coupon value) => CouponExtensions.SetCoupon(this, value);

    /// <summary>
    /// 优惠劵来源
    /// </summary>
    [Comment("优惠劵来源")]
    public UserCouponSource Source { get; set; }

    /// <summary>
    /// 发行方 Id
    /// </summary>
    [Comment("发行方 Id")]
    public Guid ReleaseId { get; set; }

    /// <summary>
    /// 优惠劵 Id
    /// </summary>
    [Comment("优惠劵 Id")]
    public Guid? CouponId { get; set; }

    /// <summary>
    /// 优惠劵名称
    /// </summary>
    [Required]
    [Comment("优惠劵名称")]
    [StringLength(MaxLengths.CouponName)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 优惠劵类型
    /// </summary>
    [Comment("优惠劵类型")]
    public CouponType CouponType { get; set; }

    #region Effective 有效期

    /// <summary>
    /// 有效开始时间
    /// </summary>
    [Comment("有效开始时间")]
    public DateTimeOffset EffectiveStartTime { get; set; }

    /// <summary>
    /// 有效结束时间
    /// </summary>
    [Comment("有效结束时间")]
    public DateTimeOffset EffectiveEndTime { get; set; }

    #endregion

    #region Restricted 限定

    /// <summary>
    /// 限定费用类型 Id
    /// </summary>
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
    /// 用户 Id
    /// </summary>
    [Comment("用户 Id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 优惠劵状态
    /// </summary>
    [Comment("优惠劵状态")]
    public UserCouponStatus Status { get; set; }

    /// <summary>
    /// 领取时间
    /// </summary>
    [Comment("领取时间")]
    public DateTimeOffset PickUpTime { get; set; }

    /// <summary>
    /// 使用时间
    /// </summary>
    [Comment("使用时间")]
    public DateTimeOffset UsageTime { get; set; }

    /// <summary>
    /// 优惠卷
    /// </summary>
    public virtual Coupon Coupon { get; set; } = null!;

    public virtual List<OrderPaymentComposition> OrderPaymentCompositions { get; set; } = null!;

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<UserCouponInfo>
    {
        public sealed override void Configure(EntityTypeBuilder<UserCouponInfo> builder)
        {
            base.Configure(builder);

            builder.HasIndex(x => x.Source);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.CouponType);

            builder.HasOne(x => x.RestrictedFeeType)
                   .WithMany()
                   .HasForeignKey(x => x.RestrictedFeeTypeId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}