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
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("查询管理后台菜单树结构（仅支持二级）");

        // 增删改查
        routeGroup.MapGet("/{id}", async (HttpContext context, [FromRoute] Guid id) =>
        {
            var r = await Get(context, id);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Detail)
        .WithDescription("查询管理后台菜单详情");
        routeGroup.MapPost("", async (HttpContext context, [FromBody] BMMenuEdit model) =>
        {
            var r = await Post(context, model);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增管理后台菜单");
        routeGroup.MapDelete("/{id}", async (HttpContext context, [FromRoute] Guid id) =>
        {
            var r = await Delete(context, id);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Delete)
        .WithDescription("删除管理后台菜单");
        routeGroup.MapPut("", async (HttpContext context, [FromBody] BMMenuEdit model) =>
        {
            var r = await Put(context, model);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("编辑管理后台菜单");

        // 菜单权限
        routeGroup.MapGet("roletree", async (HttpContext context) =>
        {
            var r = await RoleTree(context);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("查询管理后台菜单权限树结构（仅支持二级）");
        routeGroup.MapGet("bottons", async (HttpContext context) =>
        {
            var r = await GetButtons(context);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("获取管理后台按钮列表");
        routeGroup.MapGet("bottons/{menuId}", async (HttpContext context, [FromRoute] Guid menuId) =>
        {
            var r = await GetMenuButtons(context, menuId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("获取管理后台菜单的按钮列表");
        routeGroup.MapPost("bottons/{menuId}", async (HttpContext context, [FromRoute] Guid menuId) =>
        {
            var r = await AddMenuButtons(context, menuId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增管理后台菜单的按钮");
        routeGroup.MapPut("bottons/{menuId}", async (HttpContext context, [FromRoute] Guid menuId) =>
        {
            var r = await EditMenuButtons(context, menuId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("编辑管理后台菜单的按钮");

        routeGroup.MapGet("bottons/{roleId}/{menuId}", async (HttpContext context, [FromRoute] Guid roleId, [FromRoute] Guid menuId) =>
        {
            var r = await GetRoleMenuButtonsAsync(context, roleId, menuId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("获取管理后台菜单权限按钮列表");
        routeGroup.MapPost("bottons/{roleId}/{menuId}", async (HttpContext context, [FromRoute] Guid roleId, [FromRoute] Guid menuId, [FromBody] IEnumerable<BMButtonModel> buttons) =>
        {
            var r = await AddMenuButtons(context, roleId, menuId, buttons);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增管理后台菜单权限按钮");
        routeGroup.MapPut("bottons/{roleId}/{menuId}", async (HttpContext context, [FromRoute] Guid roleId, [FromRoute] Guid menuId, [FromQuery] string name, [FromBody] IEnumerable<BMButtonModel> buttons) =>
        {
            var r = await EditMenuButtons(context, roleId, menuId, name, buttons);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("编辑管理后台菜单权限按钮");
        routeGroup.MapDelete("bottons/{roleId}/{menuId}", async (HttpContext context, [FromRoute] Guid roleId, [FromRoute] Guid menuId) =>
        {
            var r = await DeleteMenuButtons(context, roleId, menuId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("删除管理后台菜单权限按钮");
    }

    static async Task<BMApiRsp<List<BMMenuTreeItem>?>> Tree(HttpContext context)
    {
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var r = await repo.GetTreeAsync();
        return r;
    }

    static async Task<BMApiRsp<BMMenuModel?>> Get(HttpContext context, Guid id)
    {
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var r = await repo.InfoAsync(id);
        return r;
    }

    static async Task<BMApiRsp<int>> Post(HttpContext context, BMMenuEdit model)
    {
        if (model.Id != default)
        {
            return HttpStatusCode.BadRequest;
        }

        var r = await PostOrPut(context, model);
        return r;
    }

    static async Task<BMApiRsp<int>> PostOrPut(HttpContext context, BMMenuEdit model)
    {
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var userId = context.GetACUserId();
        var tenantId = TenantConstants.RootTenantIdG;
        var (rowCount, _) = await repo.InsertOrUpdateAsync(model, userId, tenantId);
        return new BMApiRsp<int>
        {
            Code = unchecked((uint)(rowCount > 0 ? StatusCodes.Status200OK : StatusCodes.Status404NotFound)),
            Content = rowCount,
        };
    }

    static async Task<BMApiRsp<bool>> Delete(HttpContext context, Guid id)
    {
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var userId = context.GetACUserId();
        var r = await repo.DeleteMenuAsync(id, TenantConstants.RootTenantIdG);
        return new BMApiRsp<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    static async Task<BMApiRsp<int>> Put(HttpContext context, BMMenuEdit model)
    {
        if (model.Id == default)
        {
            return HttpStatusCode.BadRequest;
        }

        var r = await PostOrPut(context, model);
        return r;
    }

    #region 菜单权限

    static async Task<BMApiRsp<List<BMMenuModel>?>> RoleTree(HttpContext context)
    {
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var r = await repo.GetRoleTreeAsync();
        return r;
    }

    static async Task<BMApiRsp<List<BMButtonModel>?>> GetButtons(HttpContext context)
    {
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var r = await repo.GetButtonsAsync();
        return r;
    }

    static async Task<BMApiRsp<List<Guid>?>> GetMenuButtons(HttpContext context, Guid menuId)
    {
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var r = await repo.GetMenuButtonsAsync(menuId, TenantConstants.RootTenantIdG);
        return r;
    }

    static async Task<BMApiRsp<bool>> AddMenuButtons(HttpContext context, Guid menuId, params IEnumerable<Guid> buttons)
    {
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var r = await repo.EditMenuButtonsAsync(menuId, TenantConstants.RootTenantIdG, buttons);
        return new BMApiRsp<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    static async Task<BMApiRsp<bool>> EditMenuButtons(HttpContext context, Guid menuId, params IEnumerable<Guid> buttons)
    {
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var r = await repo.EditMenuButtonsAsync(menuId, TenantConstants.RootTenantIdG, buttons);
        return new BMApiRsp<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    static async Task<BMApiRsp<List<BMButtonModel>?>> GetRoleMenuButtonsAsync(HttpContext context, Guid roleId, Guid menuId)
    {
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var r = await repo.GetRoleMenuButtonsAsync(roleId, menuId, TenantConstants.RootTenantIdG);
        return r;
    }

    static async Task<BMApiRsp<bool>> AddMenuButtons(HttpContext context, Guid roleId, Guid menuId, params IEnumerable<BMButtonModel> buttons)
    {
        var userId = context.GetACUserId();
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var r = await repo.AddMenuButtonsAsync(userId, roleId, menuId, TenantConstants.RootTenantIdG, buttons);
        return new BMApiRsp<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    static async Task<BMApiRsp<bool>> EditMenuButtons(HttpContext context, Guid roleId, Guid menuId, string name, params IEnumerable<BMButtonModel> buttons)
    {
        var userId = context.GetACUserId();
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var r = await repo.EditMenuButtonsAsync(name, userId, roleId, menuId, TenantConstants.RootTenantIdG, buttons);
        return new BMApiRsp<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    static async Task<BMApiRsp<bool>> DeleteMenuButtons(HttpContext context, Guid roleId, Guid menuId)
    {
        var userId = context.GetACUserId();
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var r = await repo.DeleteMenuButtonsAsync(userId, roleId, menuId, TenantConstants.RootTenantIdG);
        return new BMApiRsp<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    #endregion
}
