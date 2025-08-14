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
        var routeGroup = b.MapGroup(pattern);

        // 增删改查
        routeGroup.MapGet("/tree", async (HttpContext context) =>
        {
            var r = await Tree(context);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Query);
        routeGroup.MapGet("/{id}", async (HttpContext context, [FromRoute] Guid id) =>
        {
            var r = await Get(context, id);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Detail);
        routeGroup.MapPost("", async (HttpContext context, [FromBody] ACMenuEdit model) =>
        {
            var r = await Post(context, model);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Add);
        routeGroup.MapDelete("/{id}", async (HttpContext context, [FromRoute] Guid id) =>
        {
            var r = await Delete(context, id);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Delete);
        routeGroup.MapPut("", async (HttpContext context, [FromBody] ACMenuEdit model) =>
        {
            var r = await Put(context, model);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, ACButtonType.Edit);

        // 菜单权限

        // roletree GET
        // bottons GET
        // bottons/{menuId} GET
        // bottons/{menuId} POST
        // bottons/{menuId} PUT
        // bottons/{roleId}/{menuId} GET
        // bottons/{roleId}/{menuId} POST
        // bottons/{roleId}/{menuId} PUT
        // bottons/{roleId}/{menuId} DELETE
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
}
