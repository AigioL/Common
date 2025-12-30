using AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    public async Task<PagedModel<ArticleTableItemModel>> QueryAsync(
        Guid? id,
        Guid? categoryId,
        Guid? tagId,
        string? title,
        string? authorName,
        string? introduction,
        string? content,
        DateTimeOffset?[]? createTime,
        DateTimeOffset?[]? updateTime,
        string? createUser,
        string? operatorUser,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        IQueryable<Article> query = db.Articles
            .AsNoTrackingWithIdentityResolution();
        if (id.HasValue)
            query = query.Where(x => x.Id == id.Value);
        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);
        if (tagId.HasValue)
            query = query.Where(x => x.TagRelations!.Any(r => r.TagId == tagId.Value));
        if (!string.IsNullOrEmpty(title))
            query = query.Where(x => x.Title.Contains(title));
        if (!string.IsNullOrEmpty(authorName))
            query = query.Where(x => x.AuthorName.Contains(authorName));
        if (!string.IsNullOrEmpty(introduction))
            query = query.Where(x => x.Introduction.Contains(introduction));
        if (!string.IsNullOrEmpty(content))
            query = query.Where(x => x.Content.Contains(content));
        if (createTime != null && createTime.Length == 2)
        {
            if (createTime[0].HasValue)
                query = query.Where(x => x.CreateTime >= createTime[0]);
            if (createTime[1].HasValue)
                query = query.Where(x => x.CreateTime < createTime[1]);
        }
        if (updateTime != null && updateTime.Length == 2)
        {
            if (updateTime[0].HasValue)
                query = query.Where(x => x.UpdateTime >= updateTime[0]);
            if (updateTime[1].HasValue)
                query = query.Where(x => x.UpdateTime < updateTime[1]);
        }

        if (!string.IsNullOrEmpty(createUser))
            if (ShortGuid.TryParse(createUser, out Guid createUserId))
                query = query.Where(x => x.CreateUser!.Id == createUserId);
            else
                query = query.Where(x => x.CreateUser!.NickName!.Contains(createUser));
        if (!string.IsNullOrEmpty(operatorUser))
            if (ShortGuid.TryParse(operatorUser, out Guid operatorUserId))
                query = query.Where(x => x.OperatorUser!.Id == operatorUserId);
            else
                query = query.Where(x => x.OperatorUser!.NickName!.Contains(operatorUser));
        if (!string.IsNullOrEmpty(orderBy))
        {
            query = query.OrderByPropertyName(orderBy, desc);
        }
        else
        {
            query = query.OrderByDescending(x => x.CreateTime);
        }

        var r = await query.ProjectTo<ArticleTableItemModel>(mapper.ConfigurationProvider)
            .PagingAsync(current, pageSize, cancellationToken);
        return r;
    }

    public async Task<AddOrEditArticleModel?> GetEditByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.Articles
            .AsNoTrackingWithIdentityResolution()
            .Where(a => a.Id == id)
            .ProjectTo<AddOrEditArticleModel>(mapper.ConfigurationProvider);

        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<ApiRsp> UpdateAsync(
        Guid? operatorUserId,
        AddOrEditArticleModel model,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindAsync(model.Id, cancellationToken);

        if (entity == null)
        {
            return "找不到该文章";
        }

        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var addedTags = model.TagIds.Select(tagId => new ArticleTagRelation { ArticleId = model.Id, TagId = tagId });

        // 更新文章字段
        mapper.Map(model, entity);
        entity.OperatorUserId = operatorUserId;

        // 为简单起见，删除文章原有的标签关系，重新添加新的
        await db.ArticleTagRelations
            .Where(r => r.ArticleId == model.Id)
            .ExecuteDeleteAsync(CancellationToken.None);
        await db.ArticleTagRelations
            .AddRangeAsync(addedTags, CancellationToken.None);

        await db.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<ApiRsp> InsertAsync(
        Guid? createUserId,
        AddOrEditArticleModel model,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var entity = mapper.Map<Article>(model);
        entity.Id = default;
        entity.CreateUserId = createUserId;
        var addedTags = model.TagIds.Select(tagId => new ArticleTagRelation { Article = entity, TagId = tagId });

        await db.Articles.AddAsync(entity, cancellationToken);
        await db.ArticleTagRelations.AddRangeAsync(addedTags, cancellationToken);

        await db.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<SelectItemModel<Guid>[]> GetSelectAsync(
        int takeCount = SelectItemModel.Count,
        CancellationToken cancellationToken = default)
    {
        var query = db.Articles
            .AsNoTrackingWithIdentityResolution()
            .OrderByDescending(x => x.CreateTime)
            .Select(x => new SelectItemModel<Guid>
            {
                Id = x.Id,
                Title = x.Title,
            })
            .Take(takeCount);

        var r = await query.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<ArticleOptionItemModel[]> QueryOptionsAsync(
        Guid? categoryId,
        Guid? tagId,
        string? title,
        CancellationToken cancellationToken = default)
    {
        var query = db.Articles
            .AsNoTrackingWithIdentityResolution();

        if (categoryId.HasValue)
            query = query.Where(a => a.CategoryId == categoryId);
        if (tagId.HasValue)
            query = query.Where(a => a.TagRelations!.Any(r => r.TagId == categoryId));
        if (!string.IsNullOrEmpty(title))
            query = query.Where(a => a.Title.Contains(title));

        query = query.OrderByDescending(x => x.CreateTime);

        var query2 = query.Select(x => new ArticleOptionItemModel
        {
            Id = x.Id,
            Title = x.Title,
            CategoryId = x.CategoryId,
            AuthorName = x.AuthorName,
        });

        var r = await query2.ToArrayAsync(cancellationToken);
        return r;
    }
}

partial class ArticleRepository<TDbContext> // 微服务
{
    public async Task<PagedModel<ArticleItemModel>> QueryCategoryAsync(Guid? categoryId, int current, int pageSize, CancellationToken cancellationToken = default)
    {
        IQueryable<Article> query = EntityNoTracking
            .Include(x => x.Category)
            .Include(x => x.Tags);
        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId);
        query = query.OrderByDescending(x => x.CreateTime);
        var query2 = query.Select(FExpressions.MapToItemDTO);
        var r = await query2.PagingAsync(current, pageSize, cancellationToken);
        return r;
    }

    public async Task<PagedModel<ArticleItemModel>> QueryOrderByAsync(ArticleOrderBy orderBy, Guid? categoryId, int current, int pageSize, CancellationToken cancellationToken = default)
    {
        IQueryable<Article> query = EntityNoTracking
            .Include(x => x.Category)
            .Include(x => x.Tags);
        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId);
        switch (orderBy)
        {
            case ArticleOrderBy.DateTime:
                query = query.OrderByDescending(x => x.CreateTime);
                break;
            case ArticleOrderBy.ViewCount:
                query = query.OrderByDescending(x => x.ViewCount);
                break;
        }
        var query2 = query.Select(FExpressions.MapToItemDTO);
        var r = await query2.PagingAsync(current, pageSize, cancellationToken);
        return r;
    }

    public async Task<ArticleModel?> QueryInfoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        IQueryable<Article> query = EntityNoTracking
            .Include(x => x.Category)
            .Include(x => x.Tags);
        var query2 = query.Select(FExpressions.MapToDTO);
        var r = await query2.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
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
            CreateTime = x.CreateTime,
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
            CreateTime = x.CreateTime,
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