using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Services.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.AspNetCore.PartnerCenter.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using AddOrEditM = AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Models.AddOrEditPCUserModel;
using TableItemM = AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Models.PCUserTableItem;

namespace AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Controllers;

public static partial class PCUserController
{
    const string ControllerName = ControllerConstants.PCUsers;

    public static void MapPCUsers(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/koluser")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("合作伙伴后台用户管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] Guid? id = null,
            [FromQuery] PCUserType? userType = null,
            [FromQuery] string? phoneNumber = null,
            [FromQuery] string? phoneNumberRegionCode = null,
            [FromQuery] Guid? businessId = null,
            [FromQuery] bool? disable = null,
            [FromQuery] string? createUser = null,
            [FromQuery] string? operatorUser = null,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] DateTimeOffset?[]? createTime = null,
            [FromQuery] DateTimeOffset?[]? updateTime = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var repo = context.RequestServices.GetRequiredService<IPCUserRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await repo.QueryAsync(
                id, userType, phoneNumber, phoneNumberRegionCode, businessId, disable,
                createTime, updateTime, createUser, operatorUser,
                orderBy, desc, current, pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询合作伙伴后台用户");

        routeGroup.MapPut("{id?}", async (HttpContext context,
            [FromRoute] Guid? id,
            [FromBody] AddOrEditM model) =>
        {
            if (id.HasValue)
            {
                model.Id = id.Value;
            }

            var userId = context.GetBMUserId();
            var repo = context.RequestServices.GetRequiredService<IPCUserRepository>();
            BMApiRsp r = await repo.UpdateAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("修改分页查询合作伙伴后台用户");

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AddOrEditM model) =>
        {
            var adminCenterService = context.RequestServices.GetRequiredService<IAdminCenterService>();
            var tenantId = adminCenterService.RootTenantIdG;
            var adminRoleName = adminCenterService.RoleNameAdministrator;
            var isRootTenant = tenantId == adminCenterService.RootTenantIdG;
            List<PCMenu> addMenus = new();
            adminCenterService.HandleMenus(isRootTenant, addMenus);

            var userId = context.GetBMUserId();
            var repo = context.RequestServices.GetRequiredService<IPCUserRepository>();
            BMApiRsp r = await repo.InsertAsync(model, addMenus, isRootTenant, tenantId, null, userId, null, adminRoleName, cancellationToken: context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增分页查询合作伙伴后台用户");

        routeGroup.MapPost("init", async (HttpContext context) =>
        {
            var adminCenterService = context.RequestServices.GetRequiredService<IAdminCenterService>();
            var tenantId = adminCenterService.RootTenantIdG;
            var adminRoleName = adminCenterService.RoleNameAdministrator;
            var isRootTenant = tenantId == adminCenterService.RootTenantIdG;
            List<PCMenu> addMenus = new();
            adminCenterService.HandleMenus(isRootTenant, addMenus);

            var userId = context.GetBMUserId();
            var repo = context.RequestServices.GetRequiredService<IPCUserRepository>();
            await repo.InitSysAsync(addMenus, isRootTenant, tenantId, null, userId, null, adminRoleName);
            return BMApiRsp.Ok;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("初始化合作伙伴后台预设权限与菜单");

        routeGroup.MapDelete("switch/{id}/{disable?}", async (HttpContext context,
            [FromRoute] Guid id,
            [FromRoute] bool? disable) =>
        {
            var userId = context.GetBMUserId();
            var repo = context.RequestServices.GetRequiredService<IPCUserRepository>();
            var rowCount = await repo.SwitchAsync(id, userId, disable, context.RequestAborted);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("切换分页查询合作伙伴后台用户禁用状态");
    }
}
