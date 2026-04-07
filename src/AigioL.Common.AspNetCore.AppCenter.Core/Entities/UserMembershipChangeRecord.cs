using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// 用户会员时间变更记录
/// </summary>
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class UserMembershipChangeRecord :
    CreationBaseEntity<Guid>,
    INEWSEQUENTIALID,
    INote
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    [Comment("用户 Id")]
    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;

    /// <summary>
    /// 变更方向
    /// </summary>
    [Comment("变更方向")]
    public MembershipChangeDirection MembershipChangeDirection { get; set; }

    /// <summary>
    /// 会员订阅类型
    /// </summary>
    [Comment("会员订阅类型")]
    public MembershipLicenseFlags MemberLicenseType { get; set; }

    /// <summary>
    /// 变更值
    /// </summary>
    [Comment("变更值")]
    public TimeSpan Value { get; set; }

    /// <summary>
    /// 按量付费的会员时长
    /// </summary>
    [Comment("按量付费的会员时长")]
    public TimeSpan PayAsYoGo { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    [StringLength(MaxLengths.Text)]
    public string? Note { get; set; }

    /// <summary>
    /// 变更后的实际到期时间
    /// </summary>
    [Comment("变更后的实际到期时间")]
    public DateTimeOffset CurrentRealExpireDate { get; set; }

    /// <summary>
    /// 绑定的合作伙伴用户 Id
    /// </summary>
    [Comment("绑定的合作伙伴用户 Id")]
    public Guid? BindPCUserId { get; set; }

    /// <summary>
    /// 绑定的合作伙伴用户到期时间
    /// </summary>
    [Comment("绑定的合作伙伴用户到期时间")]
    public DateTimeOffset BindPCUserExpireDate { get; set; }

    /// <summary>
    /// 是否为按量付费的时长
    /// </summary>
    [Comment("是否为按需付费的时长")]
    public bool IsPayAsYoGo { get; set; }

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<UserMembershipChangeRecord>
    {
        public sealed override void Configure(EntityTypeBuilder<UserMembershipChangeRecord> builder)
        {
            base.Configure(builder);

            builder.ToTable(IAppDbContextBase.TableNames.UserMembershipChangeRecords);

            builder.HasIndex(x => new { x.MembershipChangeDirection, x.MemberLicenseType });

            builder.HasIndex(x => x.UserId);
        }
    }
}