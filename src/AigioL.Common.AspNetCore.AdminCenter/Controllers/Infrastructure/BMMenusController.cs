using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Models.Menus;
using AigioL.Common.AspNetCore.AdminCenter.Repositories.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;

/// <summary>
/// 管理后台菜单管理
/// </summary>
public static partial class BMMenusController
{
    const string ControllerName = "SystemMenuManage";

    public static void MapBMMenus(this IEndpointRouteBuilder b, [StringSyntax("Route")] string pattern = "bm/menus")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("管理后台的菜单管理");

        routeGroup.MapGet("/tree", async (HttpContext context) =>
        {
            var r = await Tree(context);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Query)
        .WithDescription("查询管理后台菜单树结构（仅支持二级）");

        // 增删改查
        routeGroup.MapGet("/{id}", async (HttpContext context, [FromRoute] Guid id) =>
        {
            var r = await Get(context, id);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Detail)
        .WithDescription("查询管理后台菜单详情");
        routeGroup.MapPost("", async (HttpContext context, [FromBody] ACMenuEdit model) =>
        {
            var r = await Post(context, model);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Add)
        .WithDescription("新增管理后台菜单");
        routeGroup.MapDelete("/{id}", async (HttpContext context, [FromRoute] Guid id) =>
        {
            var r = await Delete(context, id);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Delete)
        .WithDescription("删除管理后台菜单");
        routeGroup.MapPut("", async (HttpContext context, [FromBody] ACMenuEdit model) =>
        {
            var r = await Put(context, model);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Edit)
        .WithDescription("编辑管理后台菜单");

        // 菜单权限
        routeGroup.MapGet("roletree", async (HttpContext context) =>
        {
            var r = await RoleTree(context);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Query)
        .WithDescription("查询管理后台菜单权限树结构（仅支持二级）");
        routeGroup.MapGet("bottons", async (HttpContext context) =>
        {
            var r = await GetButtons(context);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Query)
        .WithDescription("获取管理后台按钮列表");
        routeGroup.MapGet("bottons/{menuId}", async (HttpContext context, [FromRoute] Guid menuId) =>
        {
            var r = await GetMenuButtons(context, menuId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Query)
        .WithDescription("获取管理后台菜单的按钮列表");
        routeGroup.MapPost("bottons/{menuId}", async (HttpContext context, [FromRoute] Guid menuId) =>
        {
            var r = await AddMenuButtons(context, menuId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Add)
        .WithDescription("新增管理后台菜单的按钮");
        routeGroup.MapPut("bottons/{menuId}", async (HttpContext context, [FromRoute] Guid menuId) =>
        {
            var r = await EditMenuButtons(context, menuId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Edit)
        .WithDescription("编辑管理后台菜单的按钮");

        routeGroup.MapGet("bottons/{roleId}/{menuId}", async (HttpContext context, [FromRoute] Guid roleId, [FromRoute] Guid menuId) =>
        {
            var r = await GetRoleMenuButtonsAsync(context, roleId, menuId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Query)
        .WithDescription("获取管理后台菜单权限按钮列表");
        routeGroup.MapPost("bottons/{roleId}/{menuId}", async (HttpContext context, [FromRoute] Guid roleId, [FromRoute] Guid menuId, [FromBody] IEnumerable<ACButtonModel> buttons) =>
        {
            var r = await AddMenuButtons(context, roleId, menuId, buttons);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Add)
        .WithDescription("新增管理后台菜单权限按钮");
        routeGroup.MapPut("bottons/{roleId}/{menuId}", async (HttpContext context, [FromRoute] Guid roleId, [FromRoute] Guid menuId, [FromQuery] string name, [FromBody] IEnumerable<ACButtonModel> buttons) =>
        {
            var r = await EditMenuButtons(context, roleId, menuId, name, buttons);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Edit)
        .WithDescription("编辑管理后台菜单权限按钮");
        routeGroup.MapDelete("bottons/{roleId}/{menuId}", async (HttpContext context, [FromRoute] Guid roleId, [FromRoute] Guid menuId) =>
        {
            var r = await DeleteMenuButtons(context, roleId, menuId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Edit)
        .WithDescription("删除管理后台菜单权限按钮");
    }

    static async Task<ApiRspAC<List<ACMenuTreeItem>?>> Tree(HttpContext context)
    {
        var repo = context.RequestServices.GetRequiredService<IACMenuRepository>();
        var r = await repo.GetTreeAsync();
        return r;
    }

    static async Task<ApiRspAC<ACMenuModel?>> Get(HttpContext context, Guid id)
    {
        var repo = context.RequestServices.GetRequiredService<IACMenuRepository>();
        var r = await repo.InfoAsync(id);
        return r;
    }

    static async Task<ApiRspAC<int>> Post(HttpContext context, ACMenuEdit model)
    {
        if (model.Id != default)
        {
            return HttpStatusCode.BadRequest;
        }

        var r = await PostOrPut(context, model);
        return r;
    }

    static async Task<ApiRspAC<int>> PostOrPut(HttpContext context, ACMenuEdit model)
    {
        var repo = context.RequestServices.GetRequiredService<IACMenuRepository>();
        var userId = context.GetACUserId();
        var tenantId = TenantConstants.RootTenantIdG;
        var (rowCount, _) = await repo.InsertOrUpdateAsync(model, userId, tenantId);
        return new ApiRspAC<int>
        {
            Code = unchecked((uint)(rowCount > 0 ? StatusCodes.Status200OK : StatusCodes.Status404NotFound)),
            Content = rowCount,
        };
    }

    static async Task<ApiRspAC<bool>> Delete(HttpContext context, Guid id)
    {
        var repo = context.RequestServices.GetRequiredService<IACMenuRepository>();
        var userId = context.GetACUserId();
        var r = await repo.DeleteMenuAsync(id, TenantConstants.RootTenantIdG);
        return new ApiRspAC<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    static async Task<ApiRspAC<int>> Put(HttpContext context, ACMenuEdit model)
    {
        if (model.Id == default)
        {
            return HttpStatusCode.BadRequest;
        }

        var r = await PostOrPut(context, model);
        return r;
    }

    #region 菜单权限

    static async Task<ApiRspAC<List<ACMenuModel>?>> RoleTree(HttpContext context)
    {
        var repo = context.RequestServices.GetRequiredService<IACMenuRepository>();
        var r = await repo.GetRoleTreeAsync();
        return r;
    }

    static async Task<ApiRspAC<List<ACButtonModel>?>> GetButtons(HttpContext context)
    {
        var repo = context.RequestServices.GetRequiredService<IACMenuRepository>();
        var r = await repo.GetButtonsAsync();
        return r;
    }

    static async Task<ApiRspAC<List<Guid>?>> GetMenuButtons(HttpContext context, Guid menuId)
    {
        var repo = context.RequestServices.GetRequiredService<IACMenuRepository>();
        var r = await repo.GetMenuButtonsAsync(menuId, TenantConstants.RootTenantIdG);
        return r;
    }

    static async Task<ApiRspAC<bool>> AddMenuButtons(HttpContext context, Guid menuId, params IEnumerable<Guid> buttons)
    {
        var repo = context.RequestServices.GetRequiredService<IACMenuRepository>();
        var r = await repo.EditMenuButtonsAsync(menuId, TenantConstants.RootTenantIdG, buttons);
        return new ApiRspAC<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    static async Task<ApiRspAC<bool>> EditMenuButtons(HttpContext context, Guid menuId, params IEnumerable<Guid> buttons)
    {
        var repo = context.RequestServices.GetRequiredService<IACMenuRepository>();
        var r = await repo.EditMenuButtonsAsync(menuId, TenantConstants.RootTenantIdG, buttons);
        return new ApiRspAC<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    static async Task<ApiRspAC<List<ACButtonModel>?>> GetRoleMenuButtonsAsync(HttpContext context, Guid roleId, Guid menuId)
    {
        var repo = context.RequestServices.GetRequiredService<IACMenuRepository>();
        var r = await repo.GetRoleMenuButtonsAsync(roleId, menuId, TenantConstants.RootTenantIdG);
        return r;
    }

    static async Task<ApiRspAC<bool>> AddMenuButtons(HttpContext context, Guid roleId, Guid menuId, params IEnumerable<ACButtonModel> buttons)
    {
        var userId = context.GetACUserId();
        var repo = context.RequestServices.GetRequiredService<IACMenuRepository>();
        var r = await repo.AddMenuButtonsAsync(userId, roleId, menuId, TenantConstants.RootTenantIdG, buttons);
        return new ApiRspAC<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    static async Task<ApiRspAC<bool>> EditMenuButtons(HttpContext context, Guid roleId, Guid menuId, string name, params IEnumerable<ACButtonModel> buttons)
    {
        var userId = context.GetACUserId();
        var repo = context.RequestServices.GetRequiredService<IACMenuRepository>();
        var r = await repo.EditMenuButtonsAsync(name, userId, roleId, menuId, TenantConstants.RootTenantIdG, buttons);
        return new ApiRspAC<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    static async Task<ApiRspAC<bool>> DeleteMenuButtons(HttpContext context, Guid roleId, Guid menuId)
    {
        var userId = context.GetACUserId();
        var repo = context.RequestServices.GetRequiredService<IACMenuRepository>();
        var r = await repo.DeleteMenuButtonsAsync(userId, roleId, menuId, TenantConstants.RootTenantIdG);
        return new ApiRspAC<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    #endregion
}
