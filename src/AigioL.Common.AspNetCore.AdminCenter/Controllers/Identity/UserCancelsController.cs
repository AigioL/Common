using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Identity.Models.UserDeleteTableItem;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Identity;

/// <summary>
/// 客户端用户注销信息管理
/// </summary>
public static partial class UserCancelsController
{
    const string ControllerName = ControllerConstants.UserCancel;

    public static void MapUserCancels(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/identity/usercancels")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("客户端用户注销信息管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] Guid? userId,
            [FromQuery] string? phoneNumber,
            [FromQuery] string? email,
            [FromQuery] string? nickName,
            [FromQuery] Gender? gender,
            [FromQuery] DateTimeOffset?[]? birthDate,
            [FromQuery] int? areaId,
            [FromQuery] DateTimeOffset?[]? creationTime,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var startTime = context.GetQueryDateTimeRangeNullable("startTime");
            var endTime = context.GetQueryDateTimeRangeNullable("endTime");
            var userDeleteRepo = context.RequestServices.GetRequiredService<IUserDeleteRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await userDeleteRepo.QueryAsync(
                userId, phoneNumber, email,
                nickName, gender, birthDate,
                areaId, creationTime, orderBy,
                desc, current, pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询客户端用户注销信息");
    }
}
