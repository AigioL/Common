using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using AddOrEditM = AigioL.Common.AspNetCore.AppCenter.Basic.Models.AddOrEditKeyValuePairModel;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Basic.Models.KeyValuePairTableItemModel;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Basics;

/// <summary>
/// 键值对管理
/// </summary>
public static partial class KeyValuePairController
{
    const string ControllerName = ControllerConstants.KeyValuePair;

    public static void MapKeyValuePair(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/basics/keyvaluepair")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("键值对管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] string? id = null,
            [FromQuery] string? value = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var keyValuePairRepo = context.RequestServices.GetRequiredService<IKeyValuePairRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await keyValuePairRepo.QueryAsync(
                id, value, current,
                pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询键值对");

        routeGroup.MapPut("", async (HttpContext context,
            [FromBody] AddOrEditM model) =>
        {
            var userId = context.GetBMUserId();
            var keyValuePairRepo = context.RequestServices.GetRequiredService<IKeyValuePairRepository>();
            BMApiRsp r = await keyValuePairRepo.UpdateAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("修改键值对");

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AddOrEditM model) =>
        {
            var userId = context.GetBMUserId();
            var keyValuePairRepo = context.RequestServices.GetRequiredService<IKeyValuePairRepository>();
            BMApiRsp r = await keyValuePairRepo.InsertAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增键值对");

        routeGroup.MapDelete("{id}", async (HttpContext context,
            [FromRoute] string id) =>
        {
            var keyValuePairRepo = context.RequestServices.GetRequiredService<IKeyValuePairRepository>();
            var rowCount = await keyValuePairRepo.DeleteAsync(id);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Delete)
        .WithDescription("删除键值对（软删除）");

        routeGroup.MapDelete("{id}/physical", async (HttpContext context,
            [FromRoute] string id) =>
        {
            var keyValuePairRepo = context.RequestServices.GetRequiredService<IKeyValuePairRepository>();
            var rowCount = await keyValuePairRepo.PhysicalDeleteAsync(id);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Delete)
        .WithDescription("删除键值对（物理删除）");

        routeGroup.MapDelete("switch/{id}/{enable?}", async (HttpContext context,
            [FromRoute] string id,
            [FromRoute] bool? enable) =>
        {
            var keyValuePairRepo = context.RequestServices.GetRequiredService<IKeyValuePairRepository>();
            var rowCount = await keyValuePairRepo.SwitchAsync(id, enable);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("切换禁用或启用键值对");
    }
}
