using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;

public partial interface IAppVerRepository : IRepository<AppVer, Guid>, IEFRepository
{
    /// <summary>
    /// 获取全部版本信息
    /// </summary>
    Task<AppVer[]> GetAppVerAllAsync();
}

partial interface IAppVerRepository // 管理后台
{
    /// <summary>
    /// 表格查询
    /// </summary>
    Task<PagedModel<AppVersionTableItemModel>> QueryAsync(
        string? version,
        bool? disable,
        bool? beta = false,
        string? orderBy = null,
        bool? desc = null,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default);

    Task<AddOrEditAppVersionModel?> GetEditByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        Guid? operatorUserId,
        AddOrEditAppVersionModel model,
        CancellationToken cancellationToken = default);

    Task<bool> InsertAsync(
        Guid? createUserId,
        AddOrEditAppVersionModel model,
        CancellationToken cancellationToken = default);
}