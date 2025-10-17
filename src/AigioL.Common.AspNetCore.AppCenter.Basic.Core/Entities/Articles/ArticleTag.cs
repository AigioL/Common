using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;

/// <summary>
/// 文章标签表实体类
/// </summary>
[Table("ArticleTags")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class ArticleTag :
    OperatorBaseEntity<Guid>,
    ISoftDeleted,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 标签名
    /// </summary>
    [StringLength(MaxLengths.ArticleTagName)]
    [Required]
    [Comment("标签名")]
    public required string Name { get; set; }

    /// <inheritdoc/>
    [Comment("是否软删除")]
    public bool SoftDeleted { get; set; }

    /// <summary>
    /// 文章
    /// </summary>
    public virtual List<Article> Articles { get; set; } = null!;

    /// <summary>
    /// 文章标签关系
    /// </summary>
    public virtual List<ArticleTagRelation> Relations { get; set; } = null!;

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<ArticleTag>
    {
        public sealed override void Configure(EntityTypeBuilder<ArticleTag> builder)
        {
            base.Configure(builder);
        }
    }
}