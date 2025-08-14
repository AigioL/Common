using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Models.Menus;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AdminCenter.Entities;

/// <summary>
/// 管理后台的菜单表实体
/// </summary>
[Table("ACMenus")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class ACMenu :
    TenantBaseEntity<Guid>,
    INEWSEQUENTIALID,
    ISort,
    INote,
    IDisable,
    IRowVersion
{
    /// <summary>
    /// 按钮多语言名称
    /// </summary>
    [Required]
    [StringLength(MaxLengths.MenuKey)]
    [Comment("按钮多语言名称")]
    public required string Key { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    [Required]
    [StringLength(MaxLengths.MenuName)]
    [Comment("菜单名称")]
    public required string Name { get; set; }

    /// <summary>
    /// 父菜单 Id
    /// </summary>
    [Comment("父菜单 Id")]
    public Guid? ParentId { get; set; }

    /// <inheritdoc cref="ACMenu"/>
    public virtual ACMenu? Parent { get; set; }

    /// <summary>
    /// 菜单 Url
    /// </summary>
    [Required]
    [StringLength(MaxLengths.Url)]
    [Comment("菜单 Url")]
    public required string Url { get; set; }

    /// <summary>
    /// 图标 Url，或 IconKey，前端去根据开头是否为 http:// or https:// 自己识别
    /// </summary>
    [StringLength(MaxLengths.Url)]
    [Comment("图标 Url")]
    public string? IconUrl { get; set; }

    /// <inheritdoc/>
    [Comment("排序")]
    public long Sort { get; set; }

    /// <inheritdoc/>
    [Comment("是否禁用")]
    public bool Disable { get; set; }

    /// <summary>
    /// 打开方式
    /// </summary>
    [Comment("打开方式")]
    public ACMenuOpenMethod OpenMethod { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    /// <summary>
    /// 子级系统菜单
    /// </summary>
    public List<ACMenu>? Children { get; set; }

    /// <inheritdoc cref="ACButton"/>
    public List<ACButton>? Buttons { get; set; }

    /// <inheritdoc cref="ACMenuButton"/>
    public List<ACMenuButton>? MenuButtons { get; set; }

    /// <inheritdoc/>
    [Comment("并发令牌")]
    public uint RowVersion { get; set; }

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<ACMenu>
    {
        /// <inheritdoc/>
        public sealed override void Configure(EntityTypeBuilder<ACMenu> builder)
        {
            base.Configure(builder);

            builder.HasMany(x => x.Children)
                   .WithOne(x => x.Parent)
                   .HasForeignKey(x => x.ParentId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Buttons)
                   .WithMany(x => x.Menus)
                   .UsingEntity<ACMenuButton>(
                       x => x.HasOne(y => y.Button)
                             .WithMany(y => y.MenuButtons)
                             .HasForeignKey(y => y.ButtonId)
                             .OnDelete(DeleteBehavior.Cascade),
                       x => x.HasOne(y => y.Menu)
                             .WithMany(y => y.MenuButtons)
                             .HasForeignKey(y => y.MenuId)
                             .OnDelete(DeleteBehavior.Cascade),
                       x => x.HasKey(y => new { y.MenuId, y.ButtonId, y.TenantId })
                   );
        }
    }
}

partial class ACMenu
{
    public static Expression<Func<ACMenu, ACMenuModel>> GetExpression() => item => new()
    {
        Id = item.Id,
        Name = item.Name,
        IconUrl = item.IconUrl,
        Key = item.Key,
        Sort = item.Sort,
        ParentId = item.ParentId,
        Url = item.Url,
        Note = item.Note,
    };
}