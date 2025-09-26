namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;

/// <summary>
/// 文章项模型
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
public partial class ArticleItemModel
{
    /// <summary>
    /// 文章 Id
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public Guid Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 作者名
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// 封面图
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public string CoverUrl { get; set; } = string.Empty;

    /// <summary>
    /// 简介
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(4)]
    public string Introduction { get; set; } = string.Empty;

    /// <summary>
    /// 浏览量
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(5)]
    public long ViewCount { get; set; }

    /// <summary>
    /// 文章创建时间
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(6)]
    public DateTimeOffset CreationTime { get; set; }

    /// <summary>
    /// 文章标签
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(7)]
    public ArticleTagModel[] Tags { get; set; } = [];

    /// <summary>
    /// 文章分类
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(8)]
    public ArticleCategoryModel? Category { get; set; }
}