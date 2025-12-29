using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;

public partial interface IArticleTagRepository : IRepository<ArticleTag, Guid>, IEFRepository
{
}

partial interface IArticleTagRepository // 管理后台
{
    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="name">标签名</param>
    /// <param name="createTime">创建时间</param>
    /// <param name="updateTime">更新时间</param>
    /// <param name="createUser">创建人</param>
    /// <param name="operatorUser">操作人</param>
    /// <param name="orderBy">排序字段</param>
    /// <param name="desc">排序: false 为降序，true 为升序 </param>
    /// <param name="current">当前页码，页码从 1 开始，默认值：<see cref="IPagedModel.DefaultCurrent"/></param>
    /// <param name="pageSize">页大小，如果为 0 必定返回空集合，默认值：<see cref="IPagedModel.DefaultPageSize"/></param>
    /// <param name="cancellationToken"></param>
    /// <returns>ArticleTag分页表格查询结果数据</returns>
    Task<PagedModel<ArticleTagTableItemModel>> QueryAsync(
        string? name,
        DateTimeOffset[]? createTime,
        DateTimeOffset[]? updateTime,
        string? createUser,
        string? operatorUser,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default);

    Task<AddOrEditArticleTagModel?> GetEditByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiRsp> UpdateAsync(
        Guid? operatorUserId,
        AddOrEditArticleTagModel model,
        CancellationToken cancellationToken = default);

    Task<ApiRsp> InsertAsync(
        Guid? createUserId,
        AddOrEditArticleTagModel model,
        CancellationToken cancellationToken = default);

    Task<Guid[]> InsertAsync(
        Guid? createUserId,
        IReadOnlyCollection<string> tags,
        CancellationToken cancellationToken = default);

    Task<ArticleTagOptionItemModel[]> QueryOptionsAsync(
        CancellationToken cancellationToken = default);
}