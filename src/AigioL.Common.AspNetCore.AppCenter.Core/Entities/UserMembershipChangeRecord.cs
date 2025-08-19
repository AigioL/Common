using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// 用户会员时间变更记录
/// </summary>
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class UserMembershipChangeRecord :
    Entity<Guid>,
    INEWSEQUENTIALID,
    INote,
    ICreationTime
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
    [Comment("变更天数")]
    public int Days { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    /// <summary>
    /// 变更后的实际到期时间
    /// </summary>
    [Comment("变更后的实际到期时间")]
    public DateTimeOffset CurrentRealExpireDate { get; set; }

    /// <summary>
    /// 变更时间
    /// </summary>
    [Comment("变更时间")]
    public DateTimeOffset CreationTime { get; set; }

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<UserMembershipChangeRecord>
    {
        public void Configure(EntityTypeBuilder<UserMembershipChangeRecord> builder)
        {
            builder.ToTable(IAppDbContextBase.TableNames.UserMembershipChangeRecords);

            builder.HasIndex(x => new { x.MembershipChangeDirection, x.MemberLicenseType });

            builder.HasIndex(x => x.UserId);
        }
    }
}