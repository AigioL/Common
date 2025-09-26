namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;

/// <summary>
/// 文章模型类
/// </summary>
public partial class ArticleModel
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
    public DateTimeOffset CreationTime { get; set; }
}