using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;

public partial interface IUserDeviceRepository : IRepository<UserDevice, Guid>
{
}

partial interface IUserDeviceRepository // 管理后台
{
    /// <summary>
    /// 后台表格查询
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="nickName"></param>
    /// <param name="deviceName"></param>
    /// <param name="deviceId"></param>
    /// <param name="lastLoginTime"></param>
    /// <param name="isTrust"></param>
    /// <param name="platform"></param>
    /// <param name="current"></param>
    /// <param name="orderBy">排序字段</param>
    /// <param name="desc">排序: false 为降序，true 为升序 </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <param name="pageSize"></param>
    Task<PagedModel<UserDeviceTableItem>> QueryAsync(
        Guid? userId,
        string? nickName,
        string? deviceName,
        string? deviceId,
        DateTimeOffset?[]? lastLoginTime,
        bool? isTrust,
        DevicePlatform2? platform,
        string? orderBy = null,
        bool? desc = null,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 登出
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    Task<bool> SignOut(Guid deviceId);
}