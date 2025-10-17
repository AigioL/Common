using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// 注销用户
/// <para>此表使用自增 Id</para>
/// <para>在用户注销时候在用户表与用户资料表清空一些个人数据，将这些个人数据记录在此表中，等几个月后再由 Job 彻底删除数据</para>
/// </summary>
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class UserDelete :
    Entity<Guid>, INEWSEQUENTIALID,
    ICreationTime,
    IPhoneNumber,
    INickName
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    [Comment("用户 Id")]
    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;

    /// <summary>
    /// 锁定结束时的时间
    /// </summary>
    [Comment("锁定结束时的时间")]
    public DateTimeOffset? LockoutEnd { get; set; }

    /// <summary>
    /// 是否被锁定
    /// </summary>
    [Comment("是否被锁定")]
    public bool LockoutEnabled { get; set; }

    /// <summary>
    /// 登录尝试失败次数
    /// </summary>
    [Comment("登录尝试失败次数")]
    public int AccessFailedCount { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [Comment("手机号")]
    public string? PhoneNumber { get; set; }

    /// <inheritdoc/>
    [Comment("手机号国家或地区代码")]
    public string? PhoneNumberRegionCode { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [Comment("邮箱")]
    [StringLength(MaxLengths.Email)]
    public string? Email { get; set; }

    [Comment("昵称")]
    [StringLength(MaxLengths.CUserNickName)]
    public string? NickName { get; set; }

    /// <summary>
    /// 用户类型
    /// </summary>
    [Comment("用户类型")]
    public UserType UserType { get; set; }

    /// <summary>
    /// 个性签名
    /// </summary>
    [Comment("个性签名")]
    [StringLength(MaxLengths.CUserPersonalizedSignature)]
    public string? PersonalizedSignature { get; set; }

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
    /// 用户创建（注册）时间
    /// </summary>
    [Comment("用户创建时间")]
    public DateTimeOffset UserCreationTime { get; set; }

    /// <summary>
    /// 关联的外部账号
    /// </summary>
    public virtual List<ExternalAccount> ExternalAccounts { get; set; } = null!;

    /// <inheritdoc/>
    [Comment("是否禁用")]
    public bool Disable { get; set; }

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<UserDelete>
    {
        public void Configure(EntityTypeBuilder<UserDelete> builder)
        {
            builder.ToTable(IAppDbContextBase.TableNames.UserDeletes);
        }
    }
}