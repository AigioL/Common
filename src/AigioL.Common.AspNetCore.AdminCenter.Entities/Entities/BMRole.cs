using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AdminCenter.Entities;

/// <summary>
/// 管理后台的角色表实体
/// </summary>
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class BMRole :
    IdentityRole<Guid>,
    INEWSEQUENTIALID,
    ISort,
    IDisable,
    IDescription,
    INote
{
    /// <summary>
    /// 角色名
    /// </summary>
    [Required]
    [StringLength(MaxLengths.Name)]
    [Comment("角色名")]
    public override string? Name { get; set; }

    /// <summary>
    /// 角色名全大写字母
    /// </summary>
    [Required]
    [StringLength(MaxLengths.Name)]
    [Comment("角色名全大写字母")]
    public override string? NormalizedName { get; set; }

    /// <inheritdoc/>
    [Comment("描述")]
    public string? Description { get; set; }

    /// <inheritdoc/>
    [Comment("排序")]
    public long Sort { get; set; }

    /// <inheritdoc/>
    [Comment("是否禁用")]
    public bool Disable { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<BMRole>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<BMRole> builder)
        {
            IOperatorBaseEntity.Configure(builder);
            ICreationBaseEntity.Configure(builder);
        }
    }
}

partial class BMRole : ICreationBaseEntity<Guid>
{
    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }

    /// <inheritdoc/>
    [Comment("创建人")]
    public Guid? CreateUserId { get; set; }

    /// <inheritdoc/>
    public virtual BMUser? CreateUser { get; set; }
}

partial class BMRole : IOperatorBaseEntity<Guid>
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

partial class BMRole : ITenantBaseEntity<Guid>
{
    /// <inheritdoc/>
    [Comment("是否软删除")]
    public bool SoftDeleted { get; set; }

    /// <inheritdoc/>
    [Comment("租户 Id")]
    public Guid TenantId { get; set; }
}