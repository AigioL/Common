using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// 客户端用户表实体类
/// </summary>
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class User :
    IdentityUser<Guid>,
    INEWSEQUENTIALID,
    ICreationTime,
    IOperatorUserId,
    IUpdateTime,
    INickName,
    IPhoneNumber,
    IDisable,
    IPasswordHash,
    ISoftDeleted
{
    string DebuggerDisplay() => $"{NickName ?? UserName}, {Id}";

    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    [Comment("用户名")]
    [StringLength(MaxLengths.Name)]
    public override string? UserName { get; set; }

    /// <summary>
    /// 用户名全大写字母
    /// </summary>
    [Required]
    [Comment("用户名全大写字母")]
    [StringLength(MaxLengths.Name)]
    public override string? NormalizedUserName { get; set; }

    /// <summary>
    /// 密码哈希
    /// </summary>
    [Comment("密码哈希")]
    [StringLength(MaxLengths.Max_PasswordHash)]
    public override string? PasswordHash { get; set; }

    /// <summary>
    /// 锁定结束时的时间
    /// </summary>
    [Comment("锁定结束时的时间")]
    public override DateTimeOffset? LockoutEnd { get; set; }

    /// <summary>
    /// 是否被锁定
    /// </summary>
    [Comment("是否被锁定")]
    public override bool LockoutEnabled { get; set; }

    /// <summary>
    /// 登录尝试失败次数
    /// </summary>
    [Comment("登录尝试失败次数")]
    public override int AccessFailedCount { get; set; }

    /// <inheritdoc/>
    [Comment("手机号")]
    public override string? PhoneNumber { get; set; }

    /// <inheritdoc/>
    [Comment("手机号国家或地区代码")]
    public string? PhoneNumberRegionCode { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [Comment("邮箱")]
    [StringLength(MaxLengths.Email)]
    public override string? Email { get; set; }

    /// <summary>
    /// 用户类型
    /// </summary>
    [Comment("用户类型")]
    public UserType UserType { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [StringLength(MaxLengths.Max_CUserNickName)]
    [Comment("昵称")]
    public string? NickName { get; set; }

    /// <summary>
    /// 个性签名
    /// </summary>
    [Comment("个性签名")]
    [StringLength(MaxLengths.Max_CUserPersonalizedSignature)]
    public string? PersonalizedSignature { get; set; }

    ///// <summary>
    ///// 经验值
    ///// </summary>
    //[Comment("经验值")]
    //public long Experience { get; set; }

    /// <summary>
    /// 头像 Url
    /// </summary>
    [Comment("头像 Url")]
    [StringLength(MaxLengths.Url)]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    [Comment("性别")]
    public Gender Gender { get; set; }

    /// <summary>
    /// 出生日期
    /// </summary>
    [Comment("出生日期")]
    public DateTimeOffset? BirthDate { get; set; }

    /// <summary>
    /// 地区
    /// </summary>
    [Comment("地区")]
    public int? AreaId { get; set; }

    /// <summary>
    /// 上次登录时间
    /// </summary>
    [Comment("上次登录时间")]
    public DateTimeOffset LastLoginTime { get; set; }

    /// <summary>
    /// 最后读取官方消息时间
    /// </summary>
    [Comment("最后读取官方消息时间")]
    public DateTimeOffset? LastReadSystemMessageTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }

    /// <summary>
    /// 修改人 Id
    /// </summary>
    [Comment("修改人 Id")]
    public Guid? OperatorUserId { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    [Comment("修改时间")]
    public DateTimeOffset UpdateTime { get; set; }

    /// <summary>
    /// 用于账号注销，账号注销操作设置为软删除，
    /// 同时插入注销用户表一条记录，
    /// 然后由一个 Job 在注销账号 x 个月后真实执行删除以及触发级联删除所有该用户的数据
    /// </summary>
    [Comment("是否删除")]
    public bool SoftDeleted { get; set; }

    /// <inheritdoc/>
    [Comment("是否禁用")]
    public bool Disable { get; set; }
}

partial class User
{
    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            //builder.HasIndex(x => x.AreaId);
            //builder.HasIndex(x => x.PhoneNumber);

            //builder.Property(x => x.PhoneNumber).HasMaxLength(PhoneNumberHelper.DatabaseMaxLength);
            //builder.Property(x => x.Email).HasMaxLength(MaxLengths.Email);

            //builder.HasOne(x => x.OperatorUser)
            //    .WithMany()
            //    .HasForeignKey(x => x.OperatorUserId)
            //    .OnDelete(DeleteBehavior.SetNull);

            //builder.HasMany(u => u.Cancels)
            //    .WithOne(u => u.User)
            //    .HasForeignKey(u => u.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //builder.HasMany(u => u.Devices)
            //    .WithOne(x => x.User)
            //    .HasForeignKey(u => u.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //builder.HasMany(u => u.Messages)
            //    .WithOne(x => x.User)
            //    .HasForeignKey(u => u.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //builder.HasMany(u => u.SourceMessages)
            //    .WithOne(x => x.SourceUser)
            //    .HasForeignKey(u => u.SourceUserId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //builder.HasMany(u => u.ExpRecords)
            //    .WithOne(x => x.User)
            //    .HasForeignKey(u => u.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //builder.HasMany(u => u.WalletChangeRecords)
            //    .WithOne(x => x.User)
            //    .HasForeignKey(u => u.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //builder.HasMany(u => u.ClockInRecords)
            //    .WithOne(x => x.User)
            //    .HasForeignKey(u => u.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //builder
            //   .HasMany(p => p.RegistrationUsers)
            //   .WithOne(g => g.User)
            //   .HasForeignKey(p => p.UserId)
            //   .OnDelete(DeleteBehavior.SetNull);

            //builder
            //   .HasMany(p => p.RaffleResults)
            //   .WithOne(g => g.User)
            //   .HasForeignKey(p => p.UserId)
            //   .OnDelete(DeleteBehavior.SetNull);

            //builder.HasMany(u => u.MembershipChangeRecords)
            //    .WithOne(x => x.User)
            //    .HasForeignKey(u => u.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}