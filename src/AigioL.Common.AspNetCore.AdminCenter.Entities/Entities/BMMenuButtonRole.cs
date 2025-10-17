using AigioL.Common.Primitives.Columns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AdminCenter.Entities;

/// <summary>
/// 管理后台的菜单与按钮与角色权限关联表实体
/// </summary>
[Table("BMMenuButtonRoles")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class BMMenuButtonRole :
    ITenantId,
    IRowVersion
{
    /// <inheritdoc/>
    [Comment("租户 Id")]
    public Guid TenantId { get; set; }

    /// <summary>
    /// 角色 Id
    /// </summary>
    [Comment("角色 Id")]
    public Guid RoleId { get; set; }

    /// <summary>
    /// 菜单 Id
    /// </summary>
    [Comment("菜单 Id")]
    public Guid MenuId { get; set; }

    /// <summary>
    /// 按钮 Id
    /// </summary>
    [Comment("按钮 Id")]
    public Guid ButtonId { get; set; }

    /// <summary>
    /// 控制器名称
    /// </summary>
    [Required]
    [StringLength(MaxLengths.BMControllerName)]
    [Comment("控制器名称")]
    public required string ControllerName { get; set; }

    /// <inheritdoc/>
    [Comment("并发令牌")]
    public uint RowVersion { get; set; }

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<BMMenuButtonRole>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<BMMenuButtonRole> builder)
        {
            builder.HasKey(x => new { x.ButtonId, x.RoleId, x.MenuId, x.TenantId, x.ControllerName, });
        }
    }
}
