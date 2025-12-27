using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using AddOrEditM = AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions.AddOrEditAppVersionModel;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions.AppVersionTableItemModel;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Basics;

/// <summary>
/// 客户端版本管理
/// </summary>
public static partial class AppVersionController
{
    const string ControllerName = ControllerConstants.AppVersion;

    public static void MapAppVersion(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/basics/appversion")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("客户端版本管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] string? version,
            [FromQuery] bool? disable,
            [FromQuery] bool? beta = false,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var appVerRepo = context.RequestServices.GetRequiredService<IAppVerRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await appVerRepo.QueryAsync(
                version, disable, beta,
                orderBy, desc, current,
                pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询客户端版本");

        routeGroup.MapGet("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var appVerRepo = context.RequestServices.GetRequiredService<IAppVerRepository>();
            BMApiRsp<AddOrEditM?> r = await appVerRepo.GetEditByIdAsync(id, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Detail)
        .WithDescription("获取客户端版本详情");

        routeGroup.MapPut("", async (HttpContext context,
            [FromBody] AddOrEditM model) =>
        {
            var userId = context.GetBMUserId();
            var appVerRepo = context.RequestServices.GetRequiredService<IAppVerRepository>();
            BMApiRsp r = await appVerRepo.UpdateAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("修改客户端版本");

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AddOrEditM model) =>
        {
            var userId = context.GetBMUserId();
            var appVerRepo = context.RequestServices.GetRequiredService<IAppVerRepository>();
            BMApiRsp r = await appVerRepo.InsertAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增客户端版本");

        routeGroup.MapDelete("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var appVerRepo = context.RequestServices.GetRequiredService<IAppVerRepository>();
            var rowCount = await appVerRepo.DeleteAsync(id);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Delete)
        .WithDescription("删除客户端版本");
    }
}
