using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Identity.Models.AuthMessageRecordTableItem;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Identity;

/// <summary>
/// 验证码记录管理
/// </summary>
public static partial class AuthMessageRecordController
{
    const string ControllerName = ControllerConstants.AuthMessageRecord;

    public static void MapAuthMessageRecord(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/identity/authmessagerecord")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("验证码记录管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] Guid? userId,
            [FromQuery] string? phoneNumber,
            [FromQuery] string? phoneNumberRegionCode,
            [FromQuery] string? nickName,
            [FromQuery] string? email,
            [FromQuery] SmsCodeType? requestType,
            [FromQuery] bool? everCheck,
            [FromQuery] bool? checkSuccess,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var createTime = context.GetQueryDateTimeRangeNullable("createTime");
            var startTime = context.GetQueryDateTimeRangeNullable("startTime");
            var endTime = context.GetQueryDateTimeRangeNullable("endTime");
            var authMessageRecordRepo = context.RequestServices.GetRequiredService<IAuthMessageRecordRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await authMessageRecordRepo.QueryAsync(
                userId, phoneNumber, phoneNumberRegionCode,
                nickName, createTime, email,
                requestType, everCheck, checkSuccess,
                orderBy, desc, current,
                pageSize, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询验证码记录");
    }
}
