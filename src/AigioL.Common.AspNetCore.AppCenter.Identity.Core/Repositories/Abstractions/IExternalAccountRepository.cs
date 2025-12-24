using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;

public partial interface IExternalAccountRepository : IRepository<ExternalAccount, Guid>
{
}

partial interface IExternalAccountRepository // 管理后台
{
    /// <summary>
    /// 后台表格查询
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="externalAccountId"></param>
    /// <param name="type"></param>
    /// <param name="nickName"></param>
    /// <param name="givenName"></param>
    /// <param name="surname"></param>
    /// <param name="gender"></param>
    /// <param name="email"></param>
    /// <param name="userNickName"></param>
    /// <param name="creationTime"></param>
    /// <param name="updateTime"></param>
    /// <param name="orderBy">排序字段</param>
    /// <param name="desc">排序: false 为降序，true 为升序 </param>
    /// <param name="current"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PagedModel<ExternalAccountTableItem>> QueryAsync(
        Guid? userId,
        string? externalAccountId,
        ExternalLoginChannel? type,
        string? nickName,
        string? givenName,
        string? surname,
        Gender? gender,
        string? email,
        string? userNickName,
        DateTimeOffset?[]? creationTime,
        DateTimeOffset?[]? updateTime,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default);
}