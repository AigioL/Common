namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;

/// <summary>
/// 文章排序规则
/// </summary>
public enum ArticleOrderBy
{
    /// <summary>
    /// 时间倒序
    /// </summary>
    DateTime = 1,

    /// <summary>
    /// 浏览量倒序
    /// </summary>
    ViewCount = 2,
}