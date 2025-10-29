using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;

public partial interface IArticleRepository : IRepository<Article, Guid>, IEFRepository
{
}

partial interface IArticleRepository // 管理后台
{

}

partial interface IArticleRepository // 微服务
{
    Task<PagedModel<ArticleItemModel>> QueryCategoryAsync(Guid? categoryId, int current, int pageSize);

    /// <summary>
    /// 客户端-首页-查询-最新文章（按指定排序）
    /// </summary>
    /// <param name="orderBy">排序方式</param>
    /// <param name="categoryId"></param>
    /// <param name="current"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    Task<PagedModel<ArticleItemModel>> QueryOrderByAsync(ArticleOrderBy orderBy, Guid? categoryId, int current, int pageSize);

    Task<ArticleModel?> QueryInfoAsync(Guid id);

    /// <summary>
    /// 添加文章浏览量
    /// </summary>
    /// <param name="id"></param>
    /// <param name="viewCount"></param>
    /// <returns></returns>
    Task<int> AddViewCountAsync(Guid id, long viewCount);
}