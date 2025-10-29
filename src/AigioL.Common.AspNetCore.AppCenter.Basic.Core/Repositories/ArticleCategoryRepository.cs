using AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
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