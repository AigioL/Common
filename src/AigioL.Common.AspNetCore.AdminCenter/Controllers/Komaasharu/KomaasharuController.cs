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
            .WithDescription("广告管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] string? name,
            [FromQuery] KomaasharuType? type,
            [FromQuery] KomaasharuOrientation? orientation,
            [FromQuery] bool? expired,
            [FromQuery] bool? disable,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var startTime = context.GetQueryDateTimeRangeNullable("startTime");
            var endTime = context.GetQueryDateTimeRangeNullable("endTime");
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
        .WithDescription("查询广告详情");
        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AddOrEditM model) =>
        {
            var advertisementRepo = context.RequestServices.GetRequiredService<IKomaasharuRepository>();
            var rowCount = await advertisementRepo.InsertOrUpdateAsync(model);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增广告项");
        routeGroup.MapPut("", async (HttpContext context,
            [FromBody] AddOrEditM model) =>
        {
            var advertisementRepo = context.RequestServices.GetRequiredService<IKomaasharuRepository>();
            var rowCount = await advertisementRepo.InsertOrUpdateAsync(model);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
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
        .WithDescription("查询广告详情");
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
