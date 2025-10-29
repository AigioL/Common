using AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories;

sealed partial class ArticleRepository<TDbContext> :
    Repository<TDbContext, Article, Guid>,
    IArticleRepository
    where TDbContext : DbContext, IArticleDbContext
{

    public ArticleRepository(
        TDbContext dbContext,
        IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }
}

partial class ArticleRepository<TDbContext> // 管理后台
{

}

partial class ArticleRepository<TDbContext> // 微服务
{
    public async Task<PagedModel<ArticleItemModel>> QueryCategoryAsync(Guid? categoryId, int current, int pageSize)
    {
        IQueryable<Article> query = EntityNoTracking
            .Include(x => x.Category)
            .Include(x => x.Tags);
        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId);
        query = query.OrderByDescending(x => x.CreationTime);
        var query2 = query.Select(FExpressions.MapToItemDTO);
        var r = await query2.PagingAsync(current, pageSize, RequestAborted);
        return r;
    }

    public async Task<PagedModel<ArticleItemModel>> QueryOrderByAsync(ArticleOrderBy orderBy, Guid? categoryId, int current, int pageSize)
    {
        IQueryable<Article> query = EntityNoTracking
            .Include(x => x.Category)
            .Include(x => x.Tags);
        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId);
        switch (orderBy)
        {
            case ArticleOrderBy.DateTime:
                query = query.OrderByDescending(x => x.CreationTime);
                break;
            case ArticleOrderBy.ViewCount:
                query = query.OrderByDescending(x => x.ViewCount);
                break;
        }
        var query2 = query.Select(FExpressions.MapToItemDTO);
        var r = await query2.PagingAsync(current, pageSize, RequestAborted);
        return r;
    }

    public async Task<ArticleModel?> QueryInfoAsync(Guid id)
    {
        IQueryable<Article> query = EntityNoTracking
            .Include(x => x.Category)
            .Include(x => x.Tags);
        var query2 = query.Select(FExpressions.MapToDTO);
        var r = await query2.FirstOrDefaultAsync(x => x.Id == id, RequestAborted);
        return r;
    }

    public async Task<int> AddViewCountAsync(Guid id, long viewCount)
    {
        var r = await db.Articles.Where(x => x.Id == id).ExecuteUpdateAsync(
            x => x.SetProperty(static v => v.ViewCount, v => v.ViewCount + viewCount));
        return r;
    }
}

file static class FExpressions
{
    internal static readonly Expression<Func<Article, ArticleItemModel>> MapToItemDTO =
        x => new ArticleItemModel
        {
            Id = x.Id,
            Title = x.Title,
            AuthorName = x.AuthorName,
            CoverUrl = x.CoverUrl,
            Introduction = x.Introduction,
            ViewCount = x.ViewCount,
            CreationTime = x.CreationTime,
            Tags = x.Tags.Select(t => new ArticleTagModel
            {
                Id = t.Id,
                Name = t.Name,
            }).ToArray(),
            Category = x.Category == null ? null : new()
            {
                Id = x.Category.Id,
                Name = x.Category.Name,
                ParentId = x.Category.ParentId,
            },
        };

    internal static readonly Expression<Func<Article, ArticleModel>> MapToDTO =
        x => new ArticleModel
        {
            Id = x.Id,
            Title = x.Title,
            AuthorName = x.AuthorName,
            Content = x.Content,
            ViewCount = x.ViewCount,
            CreationTime = x.CreationTime,
            Tags = x.Tags.Select(t => new ArticleTagModel
            {
                Id = t.Id,
                Name = t.Name,
            }).ToArray(),
            Category = x.Category == null ? null : new()
            {
                Id = x.Category.Id,
                Name = x.Category.Name,
                ParentId = x.Category.ParentId,
            },
        };
}