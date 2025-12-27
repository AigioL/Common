using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;

public partial interface IArticleRepository : IRepository<Article, Guid>, IEFRepository
{
}

partial interface IArticleRepository // 管理后台
{
    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="id">主键</param>
    /// <param name="categoryId">分类 Id</param>
    /// <param name="tagId">标签 Id</param>
    /// <param name="title">标题</param>
    /// <param name="authorName">作者名</param>
    /// <param name="introduction">简介</param>
    /// <param name="content">内容</param>
    /// <param name="createTime">创建时间</param>
    /// <param name="updateTime">更新时间</param>
    /// <param name="createUser">创建人</param>
    /// <param name="operatorUser">操作人</param>
    /// <param name="orderBy">排序字段</param>
    /// <param name="desc">排序: false 为降序，true 为升序 </param>
    /// <param name="current">当前页码，页码从 1 开始，默认值：<see cref="IPagedModel.DefaultCurrent"/></param>
    /// <param name="pageSize">页大小，如果为 0 必定返回空集合，默认值：<see cref="IPagedModel.DefaultPageSize"/></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Article分页表格查询结果数据</returns>
    Task<PagedModel<ArticleTableItemModel>> QueryAsync(
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
        CancellationToken cancellationToken = default);

    Task<AddOrEditArticleModel?> GetEditByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiRsp> UpdateAsync(
        Guid? operatorUserId,
        Guid id,
        AddOrEditArticleModel model,
        CancellationToken cancellationToken = default);

    Task<ApiRsp> InsertAsync(
        Guid? createUserId,
        AddOrEditArticleModel model,
        CancellationToken cancellationToken = default);

    Task<SelectItemModel<Guid>[]> GetSelectAsync(
        int takeCount = SelectItemModel.Count,
        CancellationToken cancellationToken = default);

    Task<ArticleOptionItemModel[]> QueryOptionsAsync(
        Guid? categoryId,
        Guid? tagId,
        string? title,
        CancellationToken cancellationToken = default);
}

partial interface IArticleRepository // 微服务
{
    Task<PagedModel<ArticleItemModel>> QueryCategoryAsync(Guid? categoryId, int current, int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 客户端-首页-查询-最新文章（按指定排序）
    /// </summary>
    /// <param name="orderBy">排序方式</param>
    /// <param name="categoryId"></param>
    /// <param name="current"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PagedModel<ArticleItemModel>> QueryOrderByAsync(ArticleOrderBy orderBy, Guid? categoryId, int current, int pageSize,
        CancellationToken cancellationToken = default);

    Task<ArticleModel?> QueryInfoAsync(Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加文章浏览量
    /// </summary>
    /// <param name="id"></param>
    /// <param name="viewCount"></param>
    /// <returns></returns>
    Task<int> AddViewCountAsync(Guid id, long viewCount);
}