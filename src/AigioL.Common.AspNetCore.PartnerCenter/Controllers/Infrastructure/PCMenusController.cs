using AigioL.Common.AspNetCore.AdminCenter;
using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Models.Menus;
using AigioL.Common.AspNetCore.AdminCenter.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Services.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;

namespace AigioL.Common.AspNetCore.PartnerCenter.Controllers.Infrastructure;

/// <summary>
/// 合作伙伴后台菜单管理
/// </summary>
public static partial class BMMenusController
{
    const string ControllerName = ControllerConstants.SystemMenuManage;

    public static void MapPCMenus(this IEndpointRouteBuilder b, [StringSyntax("Route")] string pattern = "pc/menus")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("合作伙伴后台的菜单管理");

        //routeGroup.MapGet("/init", async (HttpContext context) =>
        //{
        //    var userManager = context.RequestServices.GetRequiredService<UserManager<BMUser>>();
        //    var user = await userManager.GetUserAsync(context.User);
        //    if (user == null)
        //    {
        //        return HttpStatusCode.Unauthorized;
        //    }

        //    var adminCenterService = context.RequestServices.GetRequiredService<IAdminCenterService>();
        //    var isRootTenant = user.TenantId == adminCenterService.RootTenantIdG;
        //    if (!isRootTenant)
        //    {
        //        return HttpStatusCode.Unauthorized;
        //    }

        //    var addMenus = new List<BMMenu>(InfoController.GetBMMenus(isRootTenant));
        //    adminCenterService.HandleMenus(isRootTenant, addMenus);
        //    InfoController.SetUserIdAndTenantId(addMenus, user.Id, adminCenterService.RootTenantIdG);

        //    BMApiRsp<BMMenuInitModel[]?> r = addMenus.Select(x => new BMMenuInitModel
        //    {
        //        Url = x.Url,
        //        Name = x.Name,
        //        Key = x.Key,
        //        IconUrl = x.IconUrl,
        //        Sort = x.Sort,
        //        Note = x.Note,
        //    }).ToArray();
        //    return r.SetHttpContext(context);
        //}).PermissionFilter(ControllerName, PCButtonType.Query)
        //.WithDescription("获取初始化的合作伙伴后台菜单组");

        routeGroup.MapGet("/keys", async (HttpContext context) =>
        {
            var t = typeof(ControllerConstants);
            string[] keys = [.. t.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(x => x.FieldType == typeof(string))
                .Select(x => x.GetValue(null)?.ToString()!)
                .Where(x => !string.IsNullOrWhiteSpace(x))];
            var r = BMApiRsp.OK(keys);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Query)
        .WithDescription("查询合作伙伴后台控制器的菜单 Key 组");

        routeGroup.MapGet("/tree", async (HttpContext context) =>
        {
            var r = await Tree(context);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Query)
        .WithDescription("查询合作伙伴后台菜单树结构（仅支持二级）");

        // 增删改查
        routeGroup.MapGet("/{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var r = await Get(context, id);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Detail)
        .WithDescription("获取合作伙伴后台菜单详情");

        routeGroup.MapPut("/sort", async (HttpContext context,
            [FromBody] BMMenuSortItem[] items) =>
        {
            var r = await MenuSort(context, items);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Edit)
        .WithDescription("设置菜单排序");

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] BMMenuEdit model) =>
        {
            var r = await PostOrPut(context, model);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Add)
        .WithDescription("新增合作伙伴后台菜单");
        routeGroup.MapDelete("/{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var r = await Delete(context, id);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Delete)
        .WithDescription("删除合作伙伴后台菜单");
        routeGroup.MapPut("{id?}", async (HttpContext context,
            [FromRoute] Guid? id,
            [FromBody] BMMenuEdit model) =>
        {
            if (id.HasValue)
            {
                model.Id = id.Value;
            }
            var r = await PostOrPut(context, model);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Edit)
        .WithDescription("编辑合作伙伴后台菜单");

        // 菜单权限
        routeGroup.MapGet("roletree", async (HttpContext context) =>
        {
            var r = await RoleTree(context);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Query)
        .WithDescription("查询合作伙伴后台菜单权限树结构（仅支持二级）");
        routeGroup.MapGet("bottons", async (HttpContext context) =>
        {
            var r = await GetButtons(context);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Query)
        .WithDescription("获取合作伙伴后台按钮列表");
        routeGroup.MapGet("bottons/{menuId}", async (HttpContext context,
            [FromRoute] Guid menuId) =>
        {
            var r = await GetMenuButtons(context, menuId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Query)
        .WithDescription("获取合作伙伴后台菜单的按钮列表");
        routeGroup.MapPost("bottons/{menuId}", async (HttpContext context,
            [FromRoute] Guid menuId,
            [FromBody] Guid[] buttons) =>
        {
            var r = await AddMenuButtons(context, menuId, buttons);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Add)
        .WithDescription("新增合作伙伴后台菜单的按钮");
        routeGroup.MapPut("bottons/{menuId}", async (HttpContext context,
            [FromRoute] Guid menuId,
            [FromBody] Guid[] buttons) =>
        {
            var r = await EditMenuButtons(context, menuId, buttons);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Edit)
        .WithDescription("编辑合作伙伴后台菜单的按钮");

        routeGroup.MapGet("bottons/{roleId}/{menuId}", async (HttpContext context,
            [FromRoute] Guid roleId,
            [FromRoute] Guid menuId) =>
        {
            var r = await GetRoleMenuButtonsAsync(context, roleId, menuId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Query)
        .WithDescription("获取合作伙伴后台菜单权限按钮列表");
        routeGroup.MapPost("bottons/{roleId}/{menuId}", async (HttpContext context,
            [FromRoute] Guid roleId,
            [FromRoute] Guid menuId,
            [FromBody] IEnumerable<BMButtonModel> buttons) =>
        {
            var r = await AddMenuButtons(context, roleId, menuId, buttons);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Add)
        .WithDescription("新增合作伙伴后台菜单权限按钮");
        routeGroup.MapPut("bottons/{roleId}/{menuId}", async (HttpContext context,
            [FromRoute] Guid roleId,
            [FromRoute] Guid menuId,
            [FromQuery] string name,
            [FromBody] IEnumerable<BMButtonModel> buttons) =>
        {
            var r = await EditMenuButtons(context, roleId, menuId, name, buttons);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Edit)
        .WithDescription("编辑合作伙伴后台菜单权限按钮");
        routeGroup.MapDelete("bottons/{roleId}/{menuId}", async (HttpContext context,
            [FromRoute] Guid roleId,
            [FromRoute] Guid menuId) =>
        {
            var r = await DeleteMenuButtons(context, roleId, menuId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Edit)
        .WithDescription("删除合作伙伴后台菜单权限按钮");
    }

    static async Task<BMApiRsp<bool>> MenuSort(HttpContext context, BMMenuSortItem[] items)
    {
        try
        {
            var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
            var r = await repo.SetMenuSort(items);
            return BMApiRsp.OkBoolean(r);
        }
        catch
        {
            return BMApiRsp.OkBoolean(false);
        }
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

    static async Task<BMApiRsp<int>> PostOrPut(HttpContext context, BMMenuEdit model)
    {
        if (model.ParentId == model.Id)
        {
            return new BMApiRsp<int>
            {
                Code = (int)HttpStatusCode.BadRequest,
                Messages = new string[] { "上级菜单不能选择自己" },
                Content = 0,
            };
        }
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var userId = context.GetBMUserId();
        var adminCenterService = context.RequestServices.GetRequiredService<IAdminCenterService>();
        var tenantId = adminCenterService.RootTenantIdG;
        var (rowCount, _) = await repo.InsertOrUpdateAsync(model, userId, tenantId);
        return new BMApiRsp<int>
        {
            Code = unchecked(StatusCodes.Status200OK),
            Content = rowCount,
        };
    }

    static async Task<BMApiRsp<bool>> Delete(HttpContext context, Guid id)
    {
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var adminCenterService = context.RequestServices.GetRequiredService<IAdminCenterService>();
        var tenantId = adminCenterService.RootTenantIdG;
        var r = await repo.DeleteMenuAsync(id, tenantId);
        return new BMApiRsp<bool>
        {
            Code = unchecked(StatusCodes.Status200OK),
            Content = r,
        };
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
        var adminCenterService = context.RequestServices.GetRequiredService<IAdminCenterService>();
        var tenantId = adminCenterService.RootTenantIdG;
        var r = await repo.GetMenuButtonsAsync(menuId, tenantId);
        return r;
    }

    static async Task<BMApiRsp<bool>> AddMenuButtons(HttpContext context, Guid menuId, params IEnumerable<Guid> buttons)
    {
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var adminCenterService = context.RequestServices.GetRequiredService<IAdminCenterService>();
        var tenantId = adminCenterService.RootTenantIdG;
        var r = await repo.EditMenuButtonsAsync(menuId, tenantId, buttons);
        return new BMApiRsp<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    static async Task<BMApiRsp<bool>> EditMenuButtons(HttpContext context, Guid menuId, params IEnumerable<Guid> buttons)
    {
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var adminCenterService = context.RequestServices.GetRequiredService<IAdminCenterService>();
        var tenantId = adminCenterService.RootTenantIdG;
        var r = await repo.EditMenuButtonsAsync(menuId, tenantId, buttons);
        return new BMApiRsp<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    static async Task<BMApiRsp<List<BMButtonModel>?>> GetRoleMenuButtonsAsync(HttpContext context, Guid roleId, Guid menuId)
    {
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var adminCenterService = context.RequestServices.GetRequiredService<IAdminCenterService>();
        var tenantId = adminCenterService.RootTenantIdG;
        var r = await repo.GetRoleMenuButtonsAsync(roleId, menuId, tenantId);
        return r;
    }

    static async Task<BMApiRsp<bool>> AddMenuButtons(HttpContext context, Guid roleId, Guid menuId, params IEnumerable<BMButtonModel> buttons)
    {
        var userId = context.GetBMUserId();
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var adminCenterService = context.RequestServices.GetRequiredService<IAdminCenterService>();
        var tenantId = adminCenterService.RootTenantIdG;
        var r = await repo.AddMenuButtonsAsync(userId, roleId, menuId, tenantId, buttons);
        return new BMApiRsp<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    static async Task<BMApiRsp<bool>> EditMenuButtons(HttpContext context, Guid roleId, Guid menuId, string name, params IEnumerable<BMButtonModel> buttons)
    {
        var userId = context.GetBMUserId();
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var adminCenterService = context.RequestServices.GetRequiredService<IAdminCenterService>();
        var tenantId = adminCenterService.RootTenantIdG;
        var r = await repo.EditMenuButtonsAsync(name, userId, roleId, menuId, tenantId, buttons);
        return new BMApiRsp<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    static async Task<BMApiRsp<bool>> DeleteMenuButtons(HttpContext context, Guid roleId, Guid menuId)
    {
        var userId = context.GetBMUserId();
        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var adminCenterService = context.RequestServices.GetRequiredService<IAdminCenterService>();
        var tenantId = adminCenterService.RootTenantIdG;
        var r = await repo.DeleteMenuButtonsAsync(userId, roleId, menuId, tenantId);
        return new BMApiRsp<bool>
        {
            Code = StatusCodes.Status200OK,
            Content = r,
        };
    }

    #endregion
}
