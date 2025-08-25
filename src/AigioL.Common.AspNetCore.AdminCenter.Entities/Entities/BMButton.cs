using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AdminCenter.Entities;

/// <summary>
/// 管理后台的按钮表实体
/// </summary>
[Table("BMButtons")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class BMButton :
    TenantBaseEntity<Guid>,
    INEWSEQUENTIALID,
    IDisable,
    IRowVersion
{
    /// <summary>
    /// 按钮名称
    /// </summary>
    [Required]
    [StringLength(MaxLengths.Name)]
    [Comment("按钮名称")]
    public required string Name { get; set; }

    /// <summary>
    /// 按钮类型
    /// </summary>
    [Comment("按钮类型")]
    public BMButtonType Type { get; set; }

    /// <inheritdoc/>
    [Comment("是否禁用")]
    public bool Disable { get; set; }

    /// <inheritdoc/>
    [Comment("并发令牌")]
    public uint RowVersion { get; set; }

    /// <inheritdoc cref="BMMenu"/>
    public virtual List<BMMenu>? Menus { get; set; }

    /// <inheritdoc cref="BMMenuButton"/>
    public virtual List<BMMenuButton>? MenuButtons { get; set; }

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<BMButton>
    {
        /// <inheritdoc/>
        public sealed override void Configure(EntityTypeBuilder<BMButton> builder)
        {
            base.Configure(builder);
        }
    }
}