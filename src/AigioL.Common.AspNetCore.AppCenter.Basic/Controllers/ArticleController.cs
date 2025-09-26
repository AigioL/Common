using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
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
            var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
            var repo = context.RequestServices.GetRequiredService<IArticleRepository>();
            var r = await Get(cache, repo, categoryId, current, pageSize, context.RequestAborted);
            return r;
        }).WithDescription("获取文章列表");
        routeGroup.MapGet("order", async (HttpContext context,
            [FromRoute] Guid? categoryId) =>
        {
            const ArticleOrderBy orderBy = ArticleOrderBy.DateTime;
            const int current = IPagedModel.DefaultCurrent;
            const int pageSize = IPagedModel.DefaultPageSize;
            var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
            var repo = context.RequestServices.GetRequiredService<IArticleRepository>();
            var r = await Order(cache, repo, categoryId, orderBy, current, pageSize, context.RequestAborted);
            return r;
        }).WithDescription("获取指定排序的文章列表，默认 DateTime 倒序");
        routeGroup.MapGet("order/{orderBy}/{current}/{pageSize}/{categoryId?}", async (HttpContext context,
            [FromRoute] Guid? categoryId,
            [FromRoute] ArticleOrderBy orderBy,
            [FromRoute] int current = IPagedModel.DefaultCurrent,
            [FromRoute] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
            var repo = context.RequestServices.GetRequiredService<IArticleRepository>();
            var r = await Order(cache, repo, categoryId, orderBy, current, pageSize, context.RequestAborted);
            return r;
        }).WithDescription("获取指定排序的文章列表，默认 DateTime 倒序");
        routeGroup.MapGet("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
            var repo = context.RequestServices.GetRequiredService<IArticleRepository>();
            var conn = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
            var r = await Info(cache, repo, conn, id, context.RequestAborted);
            return r;
        }).WithDescription("获取文章");
    }

    static async Task<ApiRsp<ArticleCategoryTreeModel[]>> GetTypes(
        IDistributedCache cache,
        IArticleCategoryRepository repo,
        CancellationToken cancellationToken = default)
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
            return r ?? [];
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
    /// <param name="cache"></param>
    /// <param name="repo"></param>
    /// <param name="categoryId">分类</param>
    /// <param name="current">页码</param>
    /// <param name="pageSize">页大小</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    static async Task<ApiRsp<PagedModel<ArticleItemModel>?>> Get(
        IDistributedCache cache,
        IArticleRepository repo,
        Guid? categoryId,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        bool useCache = true;
        if (useCache)
        {
            var cacheKey = $"{nameof(ArticleController)}_List_{categoryId}_{current}_{pageSize}";
            var r = await cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(article_memory_timeout_minutes);
                var r = await repo.QueryCategoryAsync(categoryId, current, pageSize);
                return r;
            }, cancellationToken);
            return r;
        }
        else
        {
            var r = await repo.QueryCategoryAsync(categoryId, current, pageSize);
            return r;
        }
    }

    /// <summary>
    /// 获取指定排序的文章列表，默认 DateTime 倒序
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="repo"></param>
    /// <param name="categoryId">分类 Id</param>
    /// <param name="orderBy">排序方式</param>
    /// <param name="current">页码</param>
    /// <param name="pageSize">页大小</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    static async Task<ApiRsp<PagedModel<ArticleItemModel>?>> Order(
        IDistributedCache cache,
        IArticleRepository repo,
        Guid? categoryId,
        ArticleOrderBy orderBy = ArticleOrderBy.DateTime,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        bool useCache = true;
        if (useCache)
        {
            var cacheKey = $"{nameof(ArticleController)}_Order_{categoryId}_{orderBy}_{current}_{pageSize}";
            var r = await cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(article_memory_timeout_minutes);
                var r = await repo.QueryOrderByAsync(orderBy, categoryId, current, pageSize);
                return r;
            }, cancellationToken);
            return r;
        }
        else
        {
            var r = await repo.QueryOrderByAsync(orderBy, categoryId, current, pageSize);
            return r;
        }
    }

    /// <summary>
    /// 获取文章
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="repo"></param>
    /// <param name="connection"></param>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    static async Task<ApiRsp<ArticleModel?>> Info(
        IDistributedCache cache,
        IArticleRepository repo,
        IConnectionMultiplexer connection,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        ArticleModel? r = null;
        try
        {
            bool useCache = true;
            if (useCache)
            {
                var cacheKey = $"{nameof(ArticleController)}_Info_{id}";
                r = await cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(article_memory_timeout_minutes);
                    var r = await repo.QueryInfoAsync(id);
                    return r;
                }, cancellationToken);
                return r;
            }
            else
            {
                r = await repo.QueryInfoAsync(id);
                return r;
            }
        }
        finally
        {
            if (r != null)
            {
                await CacheKeys.ArticleViewIncrementAsync(id, connection, cancellationToken);
            }
        }
    }
}
