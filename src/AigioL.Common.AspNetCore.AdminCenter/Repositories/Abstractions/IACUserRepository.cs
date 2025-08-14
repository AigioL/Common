using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;

namespace AigioL.Common.AspNetCore.AdminCenter.Repositories.Abstractions;

public interface IACUserRepository
{
    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="current"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    Task<PagedModel<ACUserTableItem>> QueryAsync(
             string? userName,
             int current = IPagedModel.DefaultCurrent,
             int pageSize = IPagedModel.DefaultPageSize);
}
