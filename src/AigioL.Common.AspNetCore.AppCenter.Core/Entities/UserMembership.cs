using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// 用户会员信息
/// </summary>
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class UserMembership :
    IEntity<Guid>,
    IUpdateTime
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Comment("用户 Id")]
    public Guid Id { get; set; }

    public virtual User User { get; set; } = null!;

    /// <summary>
    /// 首次成为会员时间，创建记录时填入
    /// </summary>
    [Comment("首次成为会员时间")]
    public DateTimeOffset FirstMembershipDate { get; set; }

    /// <summary>
    /// 会员开始时间
    /// </summary>
    [Comment("会员开始时间")]
    public DateTimeOffset StartDate { get; set; }

    /// <summary>
    /// 会员到期时间
    /// </summary>
    [Comment("会员到期时间")]
    public DateTimeOffset ExpireDate { get; set; }

    /// <summary>
    /// 会员订阅类型，跟随变更记录更新
    /// </summary>
    [Comment("会员订阅类型")]
    public MembershipLicenseFlags MemberLicenseFlags { get; set; }

    [Comment("修改时间")]
    public DateTimeOffset UpdateTime { get; set; }

    public virtual List<UserMembershipChangeRecord>? UserMembershipChangeRecords { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<UserMembership>
    {
        public void Configure(EntityTypeBuilder<UserMembership> builder)
        {
            builder.ToTable(IAppDbContextBase.TableNames.UserMemberships);

            builder.HasOne(u => u.User)
                .WithOne(u => u.Membership)
                .HasForeignKey<UserMembership>(u => u.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.UserMembershipChangeRecords)
                   .WithOne()
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
