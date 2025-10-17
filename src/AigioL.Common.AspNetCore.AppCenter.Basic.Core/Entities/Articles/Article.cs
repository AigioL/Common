using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;

/// <summary>
/// 文章表实体类
/// </summary>
[Table("Articles")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class Article :
    OperatorBaseEntity<Guid>,
    IReadOnlyTitle,
    ISoftDeleted,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 分类 Id
    /// </summary>
    [Comment("分类 Id")]
    public Guid? CategoryId { get; set; }

    /// <inheritdoc/>
    [StringLength(MaxLengths.ArticleTitle)]
    [Required]
    [Comment("标题")]
    public required string Title { get; set; }

    /// <summary>
    /// 封面 Url
    /// </summary>
    [StringLength(MaxLengths.Url)]
    [Required]
    [Comment("封面 Url")]
    public required string CoverUrl { get; set; }

    /// <summary>
    /// 作者名
    /// </summary>
    [StringLength(MaxLengths.Url)]
    [Required]
    [Comment("作者名")]
    public required string AuthorName { get; set; }

    /// <summary>
    /// 简介描述
    /// </summary>
    [Required]
    [Comment("简介描述")]
    public required string Introduction { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Required]
    [Comment("内容")]
    public required string Content { get; set; }

    /// <summary>
    /// 浏览量
    /// </summary>
    [Comment("浏览量")]
    public long ViewCount { get; set; }

    /// <inheritdoc/>
    [Comment("是否软删除")]
    public bool SoftDeleted { get; set; }

    /// <summary>
    /// 文章分类
    /// </summary>
    public virtual ArticleCategory? Category { get; set; }

    /// <summary>
    /// 文章标签关系
    /// </summary>
    public virtual List<ArticleTagRelation> TagRelations { get; set; } = null!;

    /// <summary>
    /// 文章标签
    /// </summary>
    public virtual List<ArticleTag> Tags { get; set; } = null!;

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<Article>
    {
        public sealed override void Configure(EntityTypeBuilder<Article> builder)
        {
            base.Configure(builder);

            //// 当用户删除时，该用户作为作者发布的文章应级联删除
            //builder
            //    .HasOne(x => x.Author)
            //    .WithMany(g => g.Articles)
            //    .HasForeignKey(x => x.AuthorId)
            //    .OnDelete(DeleteBehavior.Cascade);

            // 当删除一个文章分类时，【不】应级联删除该分类下所有文章
            builder
                .HasOne(x => x.Category)
                .WithMany(g => g.Articles)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // 多对多 文章与标签关系以及级联删除
            builder
                .HasMany(g => g.Tags)
                .WithMany(g => g.Articles)
                .UsingEntity<ArticleTagRelation>(
                    a => a.HasOne(x => x.Tag)
                          .WithMany(x => x.Relations)
                          .HasForeignKey(x => x.TagId)
                          .OnDelete(DeleteBehavior.Cascade),
                    a => a.HasOne(x => x.Article)
                          .WithMany(x => x.TagRelations)
                          .HasForeignKey(x => x.ArticleId)
                          .OnDelete(DeleteBehavior.Cascade),
                    a => a.HasKey(c => new { c.ArticleId, c.TagId })
                );
        }
    }
}