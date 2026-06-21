using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AntDesign;

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
        menus.AddRange(GetPCMenus(isRootTenant));
    }
    static IEnumerable<PCMenu> GetPCMenus(bool isRootTenant)
    {
        yield return new PCMenu
        {
            Url = "/Statistics",
            Name = "统计分析",
            Key = "PC_Statistics",
            IconUrl = IconType.Outline.AreaChart,
            Children = [.. GetStatistics()],
        };

        //yield return new PCMenu
        //{
        //    Url = "/Role",
        //    Name = "角色管理",
        //    Key = "PC_RoleManageMenu",
        //    IconUrl = IconType.Outline.UserSwitch,
        //    Children = [.. GetRoleManage()],
        //};

        //yield return new PCMenu
        //{
        //    Url = "/SystemManage",
        //    Name = "系统管理",
        //    Key = "PC_SystemManage",
        //    IconUrl = IconType.Outline.Control,
        //    Children = [.. GetSystemManage()],
        //};

        IEnumerable<PCMenu> GetSystemManage()
        {
            yield return new PCMenu
            {
                Url = "/System/User",
                Name = "后台用户",
                Key = ControllerConstants.PC_SystemUser,
                IconUrl = IconType.Outline.User,
            };

            yield return new PCMenu
            {
                Url = "/System/MenuManage",
                Name = "系统菜单管理",
                Key = ControllerConstants.PC_SystemMenuManage,
                IconUrl = IconType.Outline.Menu,
            };
        }

        IEnumerable<PCMenu> GetRoleManage()
        {
            yield return new PCMenu
            {
                Url = "/Role/Manage",
                Name = "角色管理",
                Key = ControllerConstants.PC_RoleManage,
                IconUrl = IconType.Outline.UserSwitch,
            };

            yield return new PCMenu
            {
                Url = "/Role/Menu",
                Name = "角色菜单管理",
                Key = "PC_RoleMenu",
                IconUrl = IconType.Outline.Menu,
            };
        }

        IEnumerable<PCMenu> GetStatistics()
        {
            yield return new PCMenu
            {
                Url = "/Statistics/OrderTrendStatistics",
                Name = "业务订单分析",
                Key = ControllerConstants.PC_BusinessOrderAnalysis,
            };
        }
    }
}