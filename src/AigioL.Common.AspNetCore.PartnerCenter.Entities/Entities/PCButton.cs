using AigioL.Common.AspNetCore.PartnerCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.PartnerCenter.Entities;

/// <summary>
/// 合作伙伴后台的按钮表实体
/// </summary>
[Table("PCButtons")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class PCButton :
    TenantPCBaseEntity<Guid>,
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
    public PCButtonType Type { get; set; }

    /// <inheritdoc/>
    [Comment("是否禁用")]
    public bool Disable { get; set; }

    /// <inheritdoc/>
    [Comment("并发令牌")]
    public uint RowVersion { get; set; }

    /// <inheritdoc cref="PCMenu"/>
    public virtual List<PCMenu>? Menus { get; set; }

    /// <inheritdoc cref="PCMenuButton"/>
    public virtual List<PCMenuButton>? MenuButtons { get; set; }

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<PCButton>
    {
        /// <inheritdoc/>
        public sealed override void Configure(EntityTypeBuilder<PCButton> builder)
        {
            base.Configure(builder);
        }
    }
}