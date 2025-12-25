using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Identity.Models.ExternalAccountTableItem;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Identity;

/// <summary>
/// 客户端用户设备管理
/// </summary>
public static partial class ExternalAccountsController
{
    const string ControllerName = ControllerConstants.ExternalAccount;

    public static void MapACUserExternalAccounts(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/identity/externalaccounts")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("客户端用户设备管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] Guid? userId,
            [FromQuery] string? externalAccountId,
            [FromQuery] ExternalLoginChannel? type,
            [FromQuery] string? nickName,
            [FromQuery] string? givenName,
            [FromQuery] string? surname,
            [FromQuery] Gender? gender,
            [FromQuery] string? email,
            [FromQuery] string? userNickName,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var createTime = context.GetQueryDateTimeRangeNullable("createTime");
            var updateTime = context.GetQueryDateTimeRangeNullable("updateTime");
            var startTime = context.GetQueryDateTimeRangeNullable("startTime");
            var endTime = context.GetQueryDateTimeRangeNullable("endTime");
            var userDeviceRepo = context.RequestServices.GetRequiredService<IExternalAccountRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await userDeviceRepo.QueryAsync(
                userId, externalAccountId, type,
                nickName, givenName, surname,
                gender, email, userNickName,
                createTime, updateTime, orderBy,
                desc, current, pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询客户端用户设备");
    }
}
