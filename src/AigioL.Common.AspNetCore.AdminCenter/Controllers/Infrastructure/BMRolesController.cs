using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Repositories.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;

/// <summary>
/// 管理后台角色权限管理
/// </summary>
public static partial class BMRolesController
{
    const string ControllerName = "RoleManage";

    public static void MapBMRoles<TRole>(this IEndpointRouteBuilder b, [StringSyntax("Route")] string pattern = "bm/roles") where TRole : BMRole, new()
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("管理后台的角色管理");

        routeGroup.MapGet("select", async (HttpContext context) =>
        {
            var r = await GetList(context);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("获取管理后台的角色下拉列表");

        // 增删改查
        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize,
            [FromQuery] string? name = null) =>
        {
            var r = await Get(context, current, pageSize, name);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("查询管理后台的角色");
        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] BMRoleModel model) =>
        {
            var tenantId = TenantConstants.RootTenantIdG;
            var r = await Post<TRole>(context, model, tenantId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增管理后台的角色");
        routeGroup.MapPut("", async (HttpContext context,
            [FromBody] BMRoleModel model) =>
        {
            var tenantId = TenantConstants.RootTenantIdG;
            var r = await Put<TRole>(context, model, tenantId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("编辑管理后台的角色");

        routeGroup.MapGet("menus/{roleId}", async (HttpContext context,
            [FromRoute] Guid roleId) =>
        {
            var tenantId = TenantConstants.RootTenantIdG;
            var r = await GetRoleMenus(context, roleId, tenantId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("获取管理后台角色的菜单主键集合");
    }

    static async Task<BMApiRsp<List<SelectItemModel<Guid>>?>> GetList(HttpContext context)
    {
        var repo = context.RequestServices.GetRequiredService<IBMRoleRepository>();
        var r = await repo.GetSelectAsync();
        return r;
    }

    static async Task<BMApiRsp<PagedModel<BMRoleModel>?>> Get(HttpContext context, int current, int pageSize, string? name = null)
    {
        var repo = context.RequestServices.GetRequiredService<IBMRoleRepository>();
        var r = await repo.QueryAsync(name, current, pageSize);
        return r;
    }

    static async Task<BMApiRsp<bool>> Post<TRole>(HttpContext context, BMRoleModel model, Guid tenantId) where TRole : BMRole, new()
    {
        var roleManager = context.RequestServices.GetRequiredService<RoleManager<TRole>>();
        var userId = context.GetACUserId();
        var role = await roleManager.FindByNameAsync(model.Name);
        if (role != null && role.TenantId != tenantId)
        {
            role = null;
        }
        if (role != null)
        {
            return $"权限名 {role.Name} 已存在";
        }
        role = new()
        {
            TenantId = tenantId,
            Name = model.Name!,
            CreateUserId = userId,
        };

        var identityResult = await roleManager.CreateAsync(role);
        if (!identityResult.Succeeded)
        {
            return identityResult;
        }
        return HttpStatusCode.OK;
    }

    static async Task<BMApiRsp<bool>> Put<TRole>(HttpContext context, BMRoleModel model, Guid tenantId) where TRole : BMRole
    {
        var roleManager = context.RequestServices.GetRequiredService<RoleManager<TRole>>();
        var role = await roleManager.FindByIdAsync(model.Id.ToString());
        if (role == null || role.TenantId != tenantId)
        {
            return HttpStatusCode.NotFound;
        }

        //role.OperatorUserId = userId;
        role.Name = model.Name!;

        var identityResult = await roleManager.UpdateAsync(role);
        if (!identityResult.Succeeded)
        {
            return identityResult;
        }
        return HttpStatusCode.OK;
    }

    static async Task<BMApiRsp<List<Guid>?>> GetRoleMenus(HttpContext context, Guid roleId, Guid tenantId)
    {
        var repo = context.RequestServices.GetRequiredService<IBMRoleRepository>();
        var r = await repo.GetRoleMenus(roleId, tenantId);
        return r;
    }
}
