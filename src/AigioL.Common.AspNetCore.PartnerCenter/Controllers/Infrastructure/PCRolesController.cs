using AigioL.Common.AspNetCore.AdminCenter;
using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Services.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.AspNetCore.PartnerCenter.Models;
using AigioL.Common.AspNetCore.PartnerCenter.Repositories.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AigioL.Common.AspNetCore.PartnerCenter.Controllers.Infrastructure;

/// <summary>
/// 合作伙伴后台角色权限管理
/// </summary>
public static partial class PCRolesController
{
    const string ControllerName = ControllerConstants.RoleManage;

    public static void MapPCRoles<TRole>(this IEndpointRouteBuilder b, [StringSyntax("Route")] string pattern = "pc/roles") where TRole : PCRole, new()
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("合作伙伴后台的角色管理");

        routeGroup.MapGet("select", async (HttpContext context) =>
        {
            var r = await GetList(context);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Query)
        .WithDescription("获取合作伙伴后台的角色下拉列表");

        // 增删改查
        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize,
            [FromQuery] string? name = null) =>
        {
            var r = await Get(context, current, pageSize, name);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Query)
        .WithDescription("查询合作伙伴后台的角色");
        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] BMRoleModel model) =>
        {
            var adminCenterService = context.RequestServices.GetRequiredService<IAdminCenterService>();
            var tenantId = adminCenterService.RootTenantIdG;
            var r = await Post<TRole>(context, model, tenantId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Add)
        .WithDescription("新增合作伙伴后台的角色");
        routeGroup.MapPut("{id?}", async (HttpContext context,
            [FromRoute] Guid? id,
            [FromBody] BMRoleModel model) =>
        {
            if (id.HasValue)
            {
                model.Id = id.Value;
            }
            var adminCenterService = context.RequestServices.GetRequiredService<IAdminCenterService>();
            var tenantId = adminCenterService.RootTenantIdG;
            var r = await Put<TRole>(context, model, tenantId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Edit)
        .WithDescription("编辑合作伙伴后台的角色");

        routeGroup.MapGet("menus/{roleId}", async (HttpContext context,
            [FromRoute] Guid roleId) =>
        {
            var adminCenterService = context.RequestServices.GetRequiredService<IAdminCenterService>();
            var tenantId = adminCenterService.RootTenantIdG;
            var r = await GetRoleMenus(context, roleId, tenantId);
            return r.SetHttpContext(context);
        }).PermissionFilter(ControllerName, PCButtonType.Query)
        .WithDescription("获取合作伙伴后台角色的菜单主键集合");
    }

    static async Task<BMApiRsp<List<SelectItemModel<Guid>>?>> GetList(HttpContext context)
    {
        var repo = context.RequestServices.GetRequiredService<IPCRoleRepository>();
        var r = await repo.GetSelectAsync();
        return r;
    }

    static async Task<BMApiRsp<PagedModel<BMRoleModel>?>> Get(HttpContext context, int current, int pageSize, string? name = null)
    {
        var repo = context.RequestServices.GetRequiredService<IPCRoleRepository>();
        var r = await repo.QueryAsync(name, current, pageSize);
        return r;
    }

    static async Task<BMApiRsp<bool>> Post<TRole>(HttpContext context, BMRoleModel model, Guid tenantId) where TRole : PCRole, new()
    {
        var roleManager = context.RequestServices.GetRequiredService<RoleManager<TRole>>();
        var userId = context.GetBMUserId();
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

    static async Task<BMApiRsp<bool>> Put<TRole>(HttpContext context, BMRoleModel model, Guid tenantId) where TRole : PCRole
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
        var repo = context.RequestServices.GetRequiredService<IPCRoleRepository>();
        var r = await repo.GetRoleMenus(roleId, tenantId);
        return r;
    }
}
