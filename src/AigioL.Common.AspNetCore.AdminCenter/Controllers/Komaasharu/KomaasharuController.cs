using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Repositories.Komaasharus.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using AddOrEditM = AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus.KomaasharuEdit;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus.KomaasharuTableItem;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Komaasharu;

/// <summary>
/// 广告管理
/// </summary>
public static partial class KomaasharuController
{
    const string ControllerName = ControllerConstants.AdvertisementManage;

    public static void MapKomaasharu(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/komaasharu")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("广告管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] string? name,
            [FromQuery] KomaasharuType? type,
            [FromQuery] KomaasharuOrientation? orientation,
            [FromQuery] bool? expired,
            [FromQuery] bool? disable,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] DateTimeOffset?[]? startTime = null,
            [FromQuery] DateTimeOffset?[]? endTime = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var advertisementRepo = context.RequestServices.GetRequiredService<IKomaasharuRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await advertisementRepo.QueryAsync(
                name, type, orientation,
                startTime, endTime, expired,
                disable, orderBy, desc,
                current, pageSize, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询广告");

        routeGroup.MapGet("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var advertisementRepo = context.RequestServices.GetRequiredService<IKomaasharuRepository>();
            BMApiRsp<AddOrEditM?> r = await advertisementRepo.GetEditByIdAsync(id, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Detail)
        .WithDescription("获取广告详情");

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AddOrEditM model) =>
        {
            model.Id = default;
            var userId = context.GetBMUserId();
            var advertisementRepo = context.RequestServices.GetRequiredService<IKomaasharuRepository>();
            BMApiRsp r = await advertisementRepo.InsertAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增广告项");

        routeGroup.MapPut("{id?}", async (HttpContext context,
            [FromRoute] Guid? id,
            [FromBody] AddOrEditM model) =>
        {
            if (id.HasValue)
            {
                model.Id = id.Value;
            }
            var userId = context.GetBMUserId();
            var advertisementRepo = context.RequestServices.GetRequiredService<IKomaasharuRepository>();
            BMApiRsp r = await advertisementRepo.UpdateAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("修改广告项");

        routeGroup.MapDelete("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var advertisementRepo = context.RequestServices.GetRequiredService<IKomaasharuRepository>();
            var rowCount = await advertisementRepo.DeleteAsync(id);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Delete)
        .WithDescription("删除广告");

        routeGroup.MapPut("setdisable/{id}/{disable}", async (HttpContext context,
            [FromRoute] Guid id,
            [FromRoute] bool disable) =>
        {
            var advertisementRepo = context.RequestServices.GetRequiredService<IKomaasharuRepository>();
            var rowCount = await advertisementRepo.SetDisableAsync(id, disable);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("设置广告禁用或启用");

        routeGroup.MapGet("perDay/{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var advertisementRepo = context.RequestServices.GetRequiredService<IKomaasharuRepository>();
            BMApiRsp<StatisticsKomaasharuResponse[]?> r = await advertisementRepo.GetStatistics(id, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("广告展示记录按天统计");
    }
}
