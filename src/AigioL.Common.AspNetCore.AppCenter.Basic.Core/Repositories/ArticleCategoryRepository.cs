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

sealed partial class ArticleCategoryRepository<TDbContext> :
    Repository<TDbContext, ArticleCategory, Guid>,
    IArticleCategoryRepository
    where TDbContext : DbContext, IArticleDbContext
{

    public ArticleCategoryRepository(
        TDbContext dbContext,
        IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }
}

partial class ArticleCategoryRepository<TDbContext> // 管理后台
{
    public async Task<PagedModel<ArticleCategoryTableItemModel>> QueryAsync(
        Guid? parentId,
        string? name,
        long? sort,
        DateTimeOffset[]? createTime,
        DateTimeOffset[]? updateTime,
        string? createUser,
        string? operatorUser,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        IQueryable<ArticleCategory> query = db.ArticleCategories.AsNoTrackingWithIdentityResolution()
           .OrderBy(x => x.Sort);
        if (parentId.HasValue)
            query = query.Where(x => x.ParentId == parentId.Value);
        if (!string.IsNullOrEmpty(name))
            query = query.Where(x => x.Name.Contains(name));
        if (sort.HasValue)
            query = query.Where(x => x.Sort == sort.Value);
        if (createTime != null)
            query = createTime.Length switch
            {
                1 => query.Where(x => x.CreateTime >= createTime[0]),
                2 => query.Where(x => x.CreateTime >= createTime[0] && x.CreateTime <= createTime[1]),
                _ => query,
            };
        if (updateTime != null)
            query = updateTime.Length switch
            {
                1 => query.Where(x => x.UpdateTime >= updateTime[0]),
                2 => query.Where(x => x.UpdateTime >= updateTime[0] && x.UpdateTime <= updateTime[1]),
                _ => query,
            };
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
            query = query.OrderBy(x => x.Sort).ThenByDescending(x => x.CreateTime);
        }
        var r = await query.ProjectTo<ArticleCategoryTableItemModel>(mapper.ConfigurationProvider)
            .PagingAsync(current, pageSize, cancellationToken);
        return r;
    }

    public async Task<AddOrEditArticleCategoryModel?> GetEditByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = db.ArticleCategories.AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == id);

        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query2 = query.ProjectTo<AddOrEditArticleCategoryModel>(mapper.ConfigurationProvider);

        var r = await query2.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<ApiRsp> UpdateAsync(
        Guid? operatorUserId,
        AddOrEditArticleCategoryModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.ParentId.HasValue)
        {
            var existing = await db.ArticleCategories
                .Where(a => a.ParentId == model.ParentId && a.Id != model.Id)
                .AnyAsync(a => a.Name == model.Name, cancellationToken);
            if (existing)
            {
                return "已存在相同名称的分类";
            }
        }
        if (model.ParentId == model.Id)
        {
            return "父分类不能是自己";
        }
        var entity = await FindAsync(model.Id, cancellationToken);
        if (entity is null)
        {
            return "找不到需要更新的数据";
        }
        if (db.ArticleCategories.Any(a => a.Id == model.ParentId && a.Parent!.Parent!.Parent != null))
        {
            return "级别不能超过 4 级";
        }

        entity.Name = model.Name;
        entity.ParentId = model.ParentId;
        entity.Sort = model.Sort;

        entity.OperatorUserId = operatorUserId;
        entity.UpdateTime = DateTimeOffset.Now;

        var r = await db.SaveChangesAsync(CancellationToken.None);
        return true;
    }


    public async Task<ApiRsp> InsertAsync(Guid? createUserId, AddOrEditArticleCategoryModel model,
        CancellationToken cancellationToken = default)
    {
        model.Id = default;

        if (model.ParentId.HasValue)
        {
            var existing = await db.ArticleCategories
                .Where(a => a.ParentId == model.ParentId)
                .AnyAsync(a => a.Name == model.Name, cancellationToken);
            if (existing)
            {
                return "已存在相同名称的分类";
            }
        }

        var (rowCount, _) = await InsertOrUpdateAsync(model, cancellationToken: CancellationToken.None);
        return rowCount > 0;
    }

    public async Task<ArticleCategoryTreeNodeModel[]> GetTreeAsync(CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.ArticleCategories
            .AsNoTrackingWithIdentityResolution()
            .OrderBy(x => x.Sort)
            .ThenBy(x => x.CreateTime);
        var query2 = query.ProjectTo<ArticleCategoryTreeNodeModel>(mapper.ConfigurationProvider);
        var categories = await query2.ToArrayAsync(cancellationToken);

        foreach (var cate in categories)
        {
            cate.Children = [.. categories.Where(c => c.ParentId == cate.Id)];
        }

        var r = categories.Where(a => a.ParentId is null).ToArray();
        return r;
    }
}

partial class ArticleCategoryRepository<TDbContext> // 微服务
{
    async Task<ArticleCategoryTreeModel[]> QueryCategoryTreeCoreAsync(Guid[]? parentIds)
    {
        IQueryable<ArticleCategory> query = EntityNoTracking.OrderBy(x => x.Sort);
        if (parentIds != null)
        {
            query = query.Where(x => x.ParentId != null && parentIds.Contains(x.ParentId.Value));
        }
        else
        {
            query = query.Where(x => x.ParentId == null);
        }
        var query2 = query.Select(FExpressions.MapToTreeDTOSingleLayer);
        var array = await query2.ToArrayAsync(RequestAborted);
        return array;
    }

    public async Task<ArticleCategoryTreeModel[]> QueryCategoryTreeAsync(short maxDepth)
    {
        // 1. 第一层
        var result = await QueryCategoryTreeCoreAsync(null);
        var current = result;
        for (short depth = 1; depth < maxDepth; depth++)
        {
            // 2. 循环最大深度，每次查询当前层所有数据的子节点
            var parentIds = result.Select(static x => x.Id).ToArray();
            var models = await QueryCategoryTreeCoreAsync(parentIds);
            var groupByModels = models.GroupBy(static x => x.ParentId);
            foreach (var m in current)
            {
                // 3. 将子节点赋值到父节点的 Child 属性
                var it = groupByModels.FirstOrDefault(x => x.Key == m.Id);
                m.Child = it == null ? [] : [.. it];
            }
            current = [.. current
                .Where(x => x.Child != null && x.Child.Length != 0)
                .SelectMany(static x => x.Child!)];
        }
        return result;
    }
}

file static class FExpressions
{
    /// <summary>
    /// 单层 ArticleCategory 到 DTO 的表达式树
    /// </summary>
    internal static readonly Expression<Func<ArticleCategory, ArticleCategoryTreeModel>> MapToTreeDTOSingleLayer =
        x => new ArticleCategoryTreeModel
        {
            Id = x.Id,
            Name = x.Name,
            ParentId = x.ParentId,
            // Child 递归在内存中处理
        };
}