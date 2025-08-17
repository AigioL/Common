using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AigioL.Common.AspNetCore.AdminCenter.Repositories.Abstractions;

public interface IBMUserRepository
{
    DatabaseFacade Database { get; }

    /// <summary>
    /// 表格查询
    /// </summary>
    Task<PagedModel<BMUserTableItem>> QueryAsync(
             string? userName, string? nickName, string? name,
             int current = IPagedModel.DefaultCurrent,
             int pageSize = IPagedModel.DefaultPageSize);
}
