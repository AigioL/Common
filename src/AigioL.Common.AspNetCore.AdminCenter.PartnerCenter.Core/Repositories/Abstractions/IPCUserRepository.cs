using AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Models;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.AspNetCore.PartnerCenter.Models;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Repositories.Abstractions;

public partial interface IPCUserRepository : IRepository<PCUser, Guid>, IEFRepository
{
    UserManager<PCUser> UserManager { get; }

    Task<PagedModel<PCUserTableItem>> QueryAsync(
        Guid? id,
        string? name,
        PCUserType? userType,
        string? phoneNumber,
        string? phoneNumberRegionCode,
        Guid? businessId,
        bool? disable,
        DateTimeOffset?[]? createTime,
        DateTimeOffset?[]? updateTime,
        string? createUser,
        string? operatorUser,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default);

    Task<ApiRsp<IdentityResult?>> UpdateAsync(
        Guid? operatorUserId,
        AddOrEditPCUserModel model,
        CancellationToken cancellationToken = default);

    Task<ApiRsp<IdentityResult?>> InsertAsync(
        AddOrEditPCUserModel model,
        List<PCMenu> addMenus,
        bool isRootTenant,
        Guid tenantId,
        string? tenantName,
        Guid? userId,
        Guid? pcUserId,
        string adminRoleName,
        HashSet<string>? addRoles = null,
        CancellationToken cancellationToken = default);

    Task<int> SwitchAsync(
        Guid primaryKey,
        Guid? operatorUserId,
        bool? disable,
        CancellationToken cancellationToken = default);
}

partial interface IPCUserRepository
{
    Task<PCUserOrderFilterModel> BuildOrderFilterAsync(
        PCUser user,
        CancellationToken cancellationToken = default);
}

partial interface IPCUserRepository // Init 初始化 PC 后台的权限与菜单
{
    /// <summary>
    /// 创建一个默认合作伙伴后台系统权限
    /// </summary>
    Task InitSysAsync(
        List<PCMenu> addMenus,
        bool isRootTenant,
        Guid tenantId,
        string? tenantName,
        Guid? userId,
        Guid? pcUserId,
        string adminRoleName,
        HashSet<string>? addRoles = null);
}