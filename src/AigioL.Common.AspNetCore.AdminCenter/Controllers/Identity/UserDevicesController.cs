using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Identity.Models.UserDeviceTableItem;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Identity;

/// <summary>
/// 客户端用户设备管理
/// </summary>
public static partial class UserDevicesController
{
    const string ControllerName = ControllerConstants.UserDevice;

    public static void MapACUserDevices(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/identity/userdevices")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("客户端用户设备管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] Guid? userId,
            [FromQuery] string? nickName,
            [FromQuery] string? deviceName,
            [FromQuery] string? deviceId,
            [FromQuery] bool? isTrust,
            [FromQuery] DevicePlatform2? platform,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var lastLoginTime = context.GetQueryDateTimeRangeNullable("lastLoginTime");
            var startTime = context.GetQueryDateTimeRangeNullable("startTime");
            var endTime = context.GetQueryDateTimeRangeNullable("endTime");
            var userDeviceRepo = context.RequestServices.GetRequiredService<IUserDeviceRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await userDeviceRepo.QueryAsync(
                userId, nickName, deviceName,
                deviceId, lastLoginTime, isTrust,
                platform, orderBy, desc,
                current, pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询客户端用户设备");

        routeGroup.MapPost("/signout/{deviceId}", async (HttpContext context,
            [FromRoute] Guid deviceId) =>
        {
            var userDeviceRepo = context.RequestServices.GetRequiredService<IUserDeviceRepository>();
            var isOK = await userDeviceRepo.SignOut(deviceId);
            var r = BMApiRsp.OkBoolean(isOK);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Delete)
        .WithDescription("登出客户端用户设备");
    }
}