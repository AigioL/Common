using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;

public partial interface IUserDeleteRepository : IRepository<UserDelete, Guid>, IEFRepository
{
    /// <summary>
    /// 删除账号（用户注销）
    /// </summary>
    Task DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default);
}

partial interface IUserDeleteRepository // 管理后台
{
    /// <summary>
    /// 后台表格查询
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="phoneNumber"></param>
    /// <param name="email"></param>
    /// <param name="nickName"></param>
    /// <param name="gender"></param>
    /// <param name="birthDate"></param>
    /// <param name="areaId"></param>
    /// <param name="createTime"></param>
    /// <param name="current"></param>
    /// <param name="pageSize"></param>
    /// <param name="orderBy">排序字段</param>
    /// <param name="desc">排序: false 为降序，true 为升序 </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PagedModel<UserDeleteTableItem>> QueryAsync(
        Guid? userId,
        string? phoneNumber,
        string? email,
        string? nickName,
        Gender? gender,
        DateTimeOffset?[]? birthDate,
        int? areaId,
        DateTimeOffset?[]? createTime,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default);
}

#if DEBUG
[Obsolete("use IUserDeleteRepository", true)]
public partial interface IUserCancelRepository : IUserDeleteRepository
{
}
#endif