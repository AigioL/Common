using AigioL.Common.AspNetCore.AdminCenter.Columns;
using AigioL.Common.Primitives.Columns;
using System.ComponentModel.DataAnnotations;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;

/// <summary>
/// 文章模型类
/// </summary>
public sealed partial class ArticleModel : IReadOnlyId<Guid>
{
    /// <summary>
    /// 主键 Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 作者名
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// 文章内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 浏览量
    /// </summary>
    public long ViewCount { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    public ArticleCategoryModel? Category { get; set; }

    /// <summary>
    /// 标签列表
    /// </summary>
    public ArticleTagModel[] Tags { get; set; } = [];

    /// <summary>
    /// 文章创建时间
    /// </summary>
    public DateTimeOffset CreateTime { get; set; }
}

public sealed partial class AddOrEditArticleModel : IReadOnlyId<Guid>
{
    /// <summary>
    /// 主键 Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 分类 Id
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [StringLength(MaxLengths.ArticleTitle)]
    public string Title { get; set; } = "";

    /// <summary>
    /// 封面 Url
    /// </summary>
    [StringLength(MaxLengths.Url)]
    public string CoverUrl { get; set; } = "";

    /// <summary>
    /// 作者名
    /// </summary>
    [StringLength(MaxLengths.Url)]
    public string AuthorName { get; set; } = "";

    /// <summary>
    /// 简介
    /// </summary>
    public string Introduction { get; set; } = "";

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; } = "";

    public Guid[] TagIds { get; set; } = Array.Empty<Guid>();
}

public sealed partial class ArticleTableItemModel : IReadOnlyId<Guid>, ICreateTime, IUpdateTime, ICreateUserIdNullable, IOperatorUserId
{
    /// <summary>
    /// 主键
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 分类 Id
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [StringLength(MaxLengths.ArticleTitle)]
    public string Title { get; set; } = "";

    /// <summary>
    /// 封面 Url
    /// </summary>
    [StringLength(MaxLengths.Url)]
    public string CoverUrl { get; set; } = "";

    /// <summary>
    /// 作者名
    /// </summary>
    [StringLength(MaxLengths.Url)]
    public string AuthorName { get; set; } = "";

    /// <summary>
    /// 简介
    /// </summary>
    public string Introduction { get; set; } = "";

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; } = "";

    /// <summary>
    /// 浏览量
    /// </summary>
    public long ViewCount { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset UpdateTime { get; set; }

    /// <summary>
    /// 创建人 UserId（创建此条目的后台管理员）
    /// </summary>
    public Guid? CreateUserId { get; set; }

    /// <summary>
    /// 创建人（创建此条目的后台管理员）
    /// </summary>
    public string? CreateUser { get; set; }

    /// <summary>
    /// 最后一次操作的人 UserId（记录后台管理员禁用或启用或编辑该条的操作）
    /// </summary>
    public Guid? OperatorUserId { get; set; }

    /// <summary>
    /// 最后一次操作的人（记录后台管理员禁用或启用或编辑该条的操作）
    /// </summary>
    public string? OperatorUser { get; set; }

    /// <summary>
    /// 文章标签
    /// </summary>
    public Guid[] TagIds { get; set; } = [];
}

public sealed partial class ArticleOptionItemModel : IReadOnlyId<Guid>
{
    /// <summary>
    /// 主键
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 分类 Id
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [StringLength(MaxLengths.ArticleTitle)]
    public string Title { get; set; } = "";

    /// <summary>
    /// 作者名
    /// </summary>
    [StringLength(MaxLengths.Url)]
    public string AuthorName { get; set; } = "";
}