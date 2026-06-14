using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;

namespace AigioL.Common.AspNetCore.AdminCenter.Services.Abstractions;

/// <summary>
/// 后台助手服务接口，由业务方实现
/// </summary>
public partial interface IAdminCenterService
{
    /// <summary>
    /// 默认管理员角色名称
    /// </summary>
    const string DefaultRoleNameAdministrator = "Administrator";

    /// <summary>
    /// 获取角色枚举类型
    /// </summary>
    Type RoleEnumType { get; }

    /// <summary>
    /// 获取根租户 Id
    /// </summary>
    Guid RootTenantIdG => TenantConstants.RootTenantIdG;

    /// <summary>
    /// 获取管理员角色名称
    /// </summary>
    string RoleNameAdministrator => DefaultRoleNameAdministrator;

    /// <summary>
    /// 获取初始化时的可添加角色名称集合
    /// </summary>
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
    List<string> AddRoles => [.. Enum.GetValues(RoleEnumType).Cast<Enum>().Select(x => x.ToString()).Where(x => x != RoleNameAdministrator)];
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

    void HandleMenus(bool isRootTenant, List<BMMenu> menus)
    {
    }
}

partial interface IAdminCenterService
{
    void HandleMenus(bool isRootTenant, List<PCMenu> menus)
    {
    }
}