using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;

/// <summary>
/// 文章分类表实体类
/// </summary>
[Table("ArticleCategories")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class ArticleCategory :
    OperatorBaseEntity<Guid>,
    ISoftDeleted,
    ISort,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 父级 Id
    /// </summary>
    [Comment("父级 Id")]
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 分类名
    /// </summary>
    [StringLength(MaxLengths.ArticleCategoryName)]
    [Required]
    [Comment("分类名")]
    public required string Name { get; set; }

    /// <inheritdoc/>
    [Comment("是否软删除")]
    public bool SoftDeleted { get; set; }

    /// <inheritdoc/>
    [Comment("排序")]
    public long Sort { get; set; }

    /// <summary>
    /// 关联的文章
    /// </summary>
    public virtual List<Article> Articles { get; set; } = null!;

    /// <summary>
    /// 父级
    /// </summary>
    public virtual ArticleCategory Parent { get; set; } = null!;

    /// <summary>
    /// 子级
    /// </summary>
    public virtual List<ArticleCategory> Children { get; set; } = null!;

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<ArticleCategory>
    {
        public sealed override void Configure(EntityTypeBuilder<ArticleCategory> builder)
        {
            base.Configure(builder);

            // 分类树状结构，级联删除
            builder
                .HasOne(p => p.Parent)
                .WithMany(g => g.Children)
                .HasForeignKey(p => p.ParentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

#if DEBUG
    /// <summary>
    /// 返回树结构文章分类
    /// </summary>
    /// <param name="articles"></param>
    /// <param name="depth">深度</param>
    /// <param name="maxDepth">最大深度</param>
    /// <returns></returns>
    [Obsolete("使用表达式树")]
    public static ArticleCategoryTreeModel MapToTreeDTO(
        ArticleCategory articles,
        int depth = 1,
        int maxDepth = 4)
    {
        dynamic destination = new
        {
            Id = articles.Id,
            Name = articles.Name,
            Child = depth <= maxDepth ? articles.Children?.Select(item => MapToTreeDTO(item, depth + 1)).ToList() : null
        };
        return default!;
    }
#endif
}
