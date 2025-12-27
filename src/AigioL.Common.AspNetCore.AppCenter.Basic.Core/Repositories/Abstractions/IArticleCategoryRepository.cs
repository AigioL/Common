using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;

/// <summary>
/// 文章分类仓储接口
/// </summary>
public partial interface IArticleCategoryRepository : IRepository<ArticleCategory, Guid>, IEFRepository
{
}

partial interface IArticleCategoryRepository // 管理后台
{
    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="parentId">父级 Id</param>
    /// <param name="name">分类名</param>
    /// <param name="sort">排序</param>
    /// <param name="createTime">创建时间</param>
    /// <param name="updateTime">更新时间</param>
    /// <param name="createUser">创建人</param>
    /// <param name="operatorUser">操作人</param>
    /// <param name="orderBy">排序字段</param>
    /// <param name="desc">排序: false 为降序，true 为升序 </param>
    /// <param name="current">当前页码，页码从 1 开始，默认值：<see cref="IPagedModel.DefaultCurrent"/></param>
    /// <param name="pageSize">页大小，如果为 0 必定返回空集合，默认值：<see cref="IPagedModel.DefaultPageSize"/></param>
    /// <param name="cancellationToken"></param>
    /// <returns>ArticleCategory分页表格查询结果数据</returns>
    Task<PagedModel<ArticleCategoryTableItemModel>> QueryAsync(
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
        CancellationToken cancellationToken = default);

    Task<AddOrEditArticleCategoryModel?> GetEditByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新一条数据
    /// </summary>
    /// <param name="operatorUserId">最后一次操作的人（记录后台管理员禁用或启用或编辑该条的操作）</param>
    /// <param name="id">主键</param>
    /// <param name="model">编辑模型</param>
    /// <param name="cancellationToken"></param>
    /// <returns>受影响的行数</returns>
    Task<ApiRsp> UpdateAsync(
        Guid? operatorUserId,
        Guid id,
        AddOrEditArticleCategoryModel model,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 新增一条数据
    /// </summary>
    /// <param name="createUserId">创建人</param>
    /// <param name="model">添加模型</param>
    /// <param name="cancellationToken"></param>
    /// <returns>受影响的行数</returns>
    Task<ApiRsp> InsertAsync(
        Guid? createUserId,
        AddOrEditArticleCategoryModel model,
        CancellationToken cancellationToken = default);

    Task<ArticleCategoryTreeNodeModel[]> GetTreeAsync(CancellationToken cancellationToken = default);
}

partial interface IArticleCategoryRepository // 微服务
{
    /// <summary>
    /// 查询文章分类嵌套模型
    /// </summary>
    /// <param name="maxDepth">最大深度</param>
    /// <returns></returns>
    Task<ArticleCategoryTreeModel[]> QueryCategoryTreeAsync(short maxDepth = 4);
}