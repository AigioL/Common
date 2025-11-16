using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO.Hashing;
using System.Runtime.CompilerServices;
using R = AigioL.Common.AspNetCore.AppCenter.Identity.UI.Properties.Resources;

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
    [Comment("用户名")]
    [StringLength(MaxLengths.Name)]
    public override string? UserName { get; set; }

    /// <summary>
    /// 用户名全大写字母
    /// </summary>
    [Comment("用户名全大写字母")]
    [StringLength(MaxLengths.Name)]
    public override string? NormalizedUserName { get; set; }

    /// <summary>
    /// 密码哈希
    /// </summary>
    [Comment("密码哈希")]
    [StringLength(MaxLengths.PasswordHash)]
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
    [StringLength(MaxLengths.CUserNickName)]
    [Comment("昵称")]
    public string? NickName { get; set; }

    /// <summary>
    /// 个性签名
    /// </summary>
    [Comment("个性签名")]
    [StringLength(MaxLengths.CUserPersonalizedSignature)]
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

    /// <inheritdoc/>
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

partial class User // Relationships
{
    /// <summary>
    /// 修改人
    /// </summary>
    public virtual BMUser OperatorUser { get; set; } = null!;

    #region User

    public virtual List<UserDelete> Deletes { get; set; } = null!;

    public virtual List<UserDevice> Devices { get; set; } = null!;

    //public virtual List<UserExpRecord> ExpRecords { get; set; } = null!;

    ///// <summary>
    ///// 当前用户接收到的消息
    ///// </summary>
    //public virtual List<UserMessage> Messages { get; set; } = null!;

    ///// <summary>
    ///// 当前用户发送的消息
    ///// </summary>
    //public virtual List<UserMessage> SourceMessages { get; set; } = null!;

    public virtual UserWallet Wallet { get; set; } = new();

    public virtual List<UserWalletChangeRecord> WalletChangeRecords { get; set; } = null!;

    //public virtual List<UserClockInRecord> ClockInRecords { get; set; } = null!;

    public virtual UserMembership Membership { get; set; } = null!;

    public virtual List<UserMembershipChangeRecord> MembershipChangeRecords { get; set; } = null!;

    /// <summary>
    /// 关联的外部账号
    /// </summary>
    public virtual List<ExternalAccount> ExternalAccounts { get; set; } = null!;

    #endregion

    public virtual List<AuthMessageRecord> AuthMessageRecords { get; set; } = null!;

    #region Order（订单）

    public virtual List<MerchantDeductionAgreement> MerchantDeductionAgreements { get; set; } = null!;

    public virtual List<MembershipBusinessOrder> MembershipBusinessOrders { get; set; } = null!;

    #endregion
}

partial class User // EntityTypeConfiguration
{
    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(p => p.UserName).IsRequired(false);
            builder.Property(p => p.NormalizedUserName).IsRequired(false);

            builder.HasIndex(x => x.AreaId);
            builder.HasIndex(x => x.PhoneNumber);
            builder.HasIndex(x => x.PhoneNumberRegionCode);

            builder.HasOne(x => x.OperatorUser)
                .WithMany()
                .HasForeignKey(x => x.OperatorUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(u => u.Deletes)
                .WithOne(u => u.User)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Devices)
                .WithOne(x => x.User)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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

            builder.HasMany(u => u.WalletChangeRecords)
                .WithOne(x => x.User)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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

            builder.HasMany(u => u.MembershipChangeRecords)
                .WithOne(x => x.User)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

public static partial class UserExtensions
{
    /// <summary>
    /// 根据用户获取昵称
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetNickName(this User user)
    {
        var value = user.GetNickNameCore();
        if (value.Length > MaxLengths.CUserNickName)
            return value[..MaxLengths.CUserNickName];
        return value;
    }

    /// <summary>
    /// 是否为生成的随机昵称
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsGeneratedNickName(this User user)
    {
        if (user.NickName == null)
            return true;

        var length = R.NewUserNickName_.Length - 3 + MaxLengths.Crc32;
        if (user.NickName.Length == length)
        {
            var mark = R.NewUserNickName_.Replace("{0}", string.Empty);
            var isContains = user.NickName.Contains(mark);
            if (isContains)
            {
                var num = user.NickName.Replace(mark, string.Empty);
                var isLetterOrDigital = num.All(x =>
                    x >= 'a' && x <= 'z' ||
                    x >= 'A' && x <= 'Z' ||
                    x >= '0' && x <= '9');
                if (isLetterOrDigital)
                {
                    return true;
                }
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string GetNickNameCore(this User user)
    {
        var value = user.NickName;

        if (string.IsNullOrEmpty(value))
            if (user.ExternalAccounts != null) // 优先取第三方账号中的昵称
            {
                value = user.ExternalAccounts
                    .Select(x => x.NickName)
                    .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                if (value != null && value.Length > MaxLengths.CUserNickName)
                {
                    value = value[..MaxLengths.CUserNickName];
                }
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

        if (string.IsNullOrEmpty(value)) // 根据主键或随机生成
        {
            if (user.Id != default)
            {
                Span<byte> hash = stackalloc byte[4];
                Crc32.Hash(user.Id.ToByteArray(), hash);
                value = Convert.ToHexStringLower(hash);
            }
            else
            {
                Span<byte> hash = stackalloc byte[4];
                Random.Shared.NextBytes(hash);
                value = Convert.ToHexStringLower(hash);
            }
            return R.NewUserNickName_.Format(value);
        }
        else
        {
            return value;
        }
    }
}