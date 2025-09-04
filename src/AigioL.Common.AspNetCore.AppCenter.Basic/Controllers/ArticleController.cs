using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Controllers;

public static partial class ArticleController
{
    /// <summary>
    /// 文章数据缓存过期时间 5 分钟
    /// </summary>
    const int article_memory_timeout_minutes = 5;

    public static void MapBasicArticle(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "basic/article")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapGet("type", async (HttpContext context) =>
        {
            var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
            var repo = context.RequestServices.GetRequiredService<IArticleCategoryRepository>();
            var r = await GetTypes(cache, repo, context.RequestAborted);
            return r;
        });
        routeGroup.MapGet("{current}/{pageSize}/{categoryId?}", async (HttpContext context,
            [FromRoute] Guid? categoryId,
            [FromRoute] int current = IPagedModel.DefaultCurrent,
            [FromRoute] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var repo = context.RequestServices.GetRequiredService<IArticleCategoryRepository>();
            var r = await Get(categoryId, current, pageSize);
            return r;
        });
        routeGroup.MapGet("order", async (HttpContext context,
            [FromRoute] Guid? categoryId) =>
        {
            const ArticleOrderBy orderBy = ArticleOrderBy.DateTime;
            const int current = IPagedModel.DefaultCurrent;
            const int pageSize = IPagedModel.DefaultPageSize;
            var repo = context.RequestServices.GetRequiredService<IArticleCategoryRepository>();
            var r = await Order(categoryId, orderBy, current, pageSize);
            return r;
        });
        routeGroup.MapGet("order/{orderBy}/{current}/{pageSize}/{categoryId?}", async (HttpContext context,
            [FromRoute] Guid? categoryId,
            [FromRoute] ArticleOrderBy orderBy,
            [FromRoute] int current = IPagedModel.DefaultCurrent,
            [FromRoute] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var repo = context.RequestServices.GetRequiredService<IArticleCategoryRepository>();
            var r = await Order(categoryId, orderBy, current, pageSize);
            return r;
        });
        routeGroup.MapGet("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var repo = context.RequestServices.GetRequiredService<IArticleCategoryRepository>();
            var r = await Info(id);
            return r;
        });
    }

    static async Task<ApiRsp<List<ArticleCategoryTreeModel>?>> GetTypes(
        IDistributedCache cache,
        IArticleCategoryRepository repo,
        CancellationToken cancellationToken)
    {
        bool useCache = true;
        if (useCache)
        {
            const string cacheKey = $"{nameof(ArticleController)}_GetTypes";
            var r = await cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(article_memory_timeout_minutes);
                var r = await repo.QueryCategoryTreeAsync();
                return r;
            }, cancellationToken);
            return r;
        }
        else
        {
            var r = await repo.QueryCategoryTreeAsync();
            return r;
        }
    }

    /// <summary>
    /// 获取文章列表
    /// </summary>
    /// <param name="categoryId">分类</param>
    /// <param name="current">页码</param>
    /// <param name="pageSize">页大小</param>
    /// <returns></returns>
    static async Task<ApiRsp<PagedModel<ArticleItemModel>?>> Get(
        Guid? categoryId,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize)
    {
        // TODO: 获取文章列表
        var cacheKey = $"{nameof(ArticleController)}_List_{categoryId}_{current}_{pageSize}";
        throw new NotImplementedException();
    }

    /// <summary>
    /// 获取指定排序的文章列表，默认 DateTime 倒叙
    /// </summary>
    /// <param name="categoryId">分类 Id</param>
    /// <param name="orderBy">排序方式</param>
    /// <param name="current">页码</param>
    /// <param name="pageSize">页大小</param>
    /// <returns></returns>
    static async Task<ApiRsp<PagedModel<ArticleItemModel>?>> Order(
        Guid? categoryId,
        ArticleOrderBy orderBy = ArticleOrderBy.DateTime,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize)
    {
        // TODO: 获取指定排序的文章列表
        var cacheKey = $"{nameof(ArticleController)}_Order_{categoryId}_{orderBy}_{current}_{pageSize}";
        throw new NotImplementedException();
    }

    /// <summary>
    /// 获取文章
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    static async Task<ApiRsp<ArticleModel?>> Info(
        Guid id)
    {
        // TODO: 获取文章
        var cacheKey = $"{nameof(ArticleController)}_Info_{id}";
        throw new NotImplementedException();
    }
}
