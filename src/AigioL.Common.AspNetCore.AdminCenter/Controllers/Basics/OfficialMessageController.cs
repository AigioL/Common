using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Notice;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using AddOrEditM = AigioL.Common.AspNetCore.AppCenter.Basic.Models.Notice.AddOrEditOfficialMessageModel;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Basic.Models.Notice.OfficialMessageTableItemModel;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Basics;

/// <summary>
/// 官方消息管理
/// </summary>
public static partial class OfficialMessageController
{
    const string ControllerName = ControllerConstants.OfficialMessage;

    public static void MapOfficialMessage(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/basics/officialmessage")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("官方消息管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] OfficialMessageType? messageType,
            [FromQuery] string? title,
            [FromQuery] ClientPlatform? pushClientDevice,
            [FromQuery] bool? userViewable,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var pushTime = context.GetQueryDateTimeRangeNullable("pushTime");
            var expireTime = context.GetQueryDateTimeRangeNullable("expireTime");
            var createTime = context.GetQueryDateTimeRangeNullable("createTime");
            var officialMessageRepo = context.RequestServices.GetRequiredService<IOfficialMessageRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await officialMessageRepo.QueryAsync(
                messageType, title, pushClientDevice,
                pushTime, expireTime, createTime,
                userViewable, orderBy, desc,
                current, pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询官方消息");

        routeGroup.MapGet("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var officialMessageRepo = context.RequestServices.GetRequiredService<IOfficialMessageRepository>();
            BMApiRsp<AddOrEditM?> r = await officialMessageRepo.GetEditByIdAsync(id, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Detail)
        .WithDescription("获取官方消息详情");

        routeGroup.MapPut("", async (HttpContext context,
            [FromBody] AddOrEditM model) =>
        {
            var userId = context.GetBMUserId();
            var officialMessageRepo = context.RequestServices.GetRequiredService<IOfficialMessageRepository>();
            BMApiRsp r = await officialMessageRepo.UpdateAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("修改官方消息");

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AddOrEditM model) =>
        {
            var userId = context.GetBMUserId();
            var officialMessageRepo = context.RequestServices.GetRequiredService<IOfficialMessageRepository>();
            BMApiRsp r = await officialMessageRepo.InsertAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增官方消息");

        routeGroup.MapDelete("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var officialMessageRepo = context.RequestServices.GetRequiredService<IOfficialMessageRepository>();
            var rowCount = await officialMessageRepo.DeleteAsync(id);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Delete)
        .WithDescription("删除官方消息");
    }
}
