using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.PartnerCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace AigioL.Common.AspNetCore.PartnerCenter.Entities;

/// <summary>
/// 合作伙伴后台的用户表实体类
/// </summary>
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class PCUser :
    IdentityUser<Guid>,
    IEntity<Guid>,
    INEWSEQUENTIALID,
    INote,
    INickName,
    IPhoneNumber,
    IDisable,
    IPasswordHash
{
    string DebuggerDisplay() => $"{NickName ?? UserName}, {Id}";

    /// <summary>
    /// 用户类型
    /// </summary>
    [Comment("用户类型")]
    public PCUserType UserType { get; set; }

    /// <summary>
    /// 关联业务 Id
    /// </summary>
    [Comment("关联业务Id")]
    [Column(TypeName = "jsonb")]
    public Guid[] BusinessIds { get; set; } = [];

    ///// <summary>
    ///// 组织架构 Id
    ///// </summary>
    //[Comment("组织架构 Id")]
    //public Guid? OrganizationalId { get; set; }

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

    /// <inheritdoc/>
    [Comment("昵称")]
    [StringLength(MaxLengths.NickName)]
    public string? NickName { get; set; }

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
    /// 性别
    /// </summary>
    [Comment("性别")]
    public Gender Gender { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    /// <inheritdoc/>
    [Comment("是否禁用")]
    public bool Disable { get; set; }

    ///// <inheritdoc cref="PCUserOrganization"/>
    //public List<PCUserOrganization>? Organizations { get; set; }

    public virtual List<AuthMessageRecord> AuthMessageRecords { get; set; } = null!;

    public virtual List<MembershipProductKeyRecord> MembershipProductKeyRecords { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<PCUser>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<PCUser> builder)
        {
            IOperatorBaseEntity.Configure(builder);
            ICreationBaseEntity.Configure(builder);
            IOperatorPCBaseEntity.Configure(builder);
            ICreationPCBaseEntity.Configure(builder);
        }
    }
}

partial class PCUser : ICreationBaseEntity<Guid>
{
    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreateTime { get; set; }

    /// <inheritdoc/>
    [Comment("创建人")]
    public Guid? CreateUserId { get; set; }

    /// <inheritdoc/>
    public virtual BMUser? CreateUser { get; set; }
}

partial class PCUser : IOperatorBaseEntity<Guid>
{
    /// <inheritdoc/>
    [Comment("更新时间")]
    public DateTimeOffset UpdateTime { get; set; }

    /// <inheritdoc/>
    [Comment("操作人")]
    public Guid? OperatorUserId { get; set; }

    /// <inheritdoc/>
    public virtual BMUser? OperatorUser { get; set; }
}

partial class PCUser : ICreationPCBaseEntity<Guid>
{
    /// <inheritdoc/>
    [Comment("创建人")]
    public Guid? CreatePCUserId { get; set; }

    /// <inheritdoc/>
    public virtual PCUser? CreatePCUser { get; set; }
}

partial class PCUser : IOperatorPCBaseEntity<Guid>
{
    /// <inheritdoc/>
    [Comment("操作人")]
    public Guid? OperatorPCUserId { get; set; }

    /// <inheritdoc/>
    public virtual PCUser? OperatorPCUser { get; set; }
}

partial class PCUser : ITenantBaseEntity<Guid>
{
    /// <inheritdoc/>
    [Comment("是否软删除")]
    public DateTimeOffset? DeleteTime { get; set; }

    /// <inheritdoc/>
    [Comment("租户 Id")]
    public Guid TenantId { get; set; }
}