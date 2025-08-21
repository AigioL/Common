using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;

/// <summary>
/// 文章标签关系表实体模类
/// </summary>
[Table("ArticleTagRelations")]
public partial class ArticleTagRelation
{
    /// <summary>
    /// 文章 Id
    /// </summary>
    [Comment("文章 Id")]
    public Guid ArticleId { get; set; }

    public virtual Article Article { get; set; } = null!;

    /// <summary>
    /// 标签 Id
    /// </summary>
    [Comment("标签 Id")]
    public Guid TagId { get; set; }

    public virtual ArticleTag Tag { get; set; } = null!;
}