using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Ordering.Models.AftersalesBillTableItem;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Ordering;

/// <summary>
/// 售后单管理
/// </summary>
public static partial class AftersalesBillController
{
    const string ControllerName = ControllerConstants.AftersalesBill;

    public static void MapAftersalesBill(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/ordering/aftersalesbills")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("售后单管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] string? orderNumber = null,
            [FromQuery] string? aftersalesNumber = null,
            [FromQuery] int? businessType = null,
            [FromQuery] Guid? userId = null,
            [FromQuery] decimal? refundAmount = null,
            [FromQuery] AuditStatus[]? auditStatus = null,
            [FromQuery] string? refundReason = null,
            [FromQuery] string? sellerNote = null,
            [FromQuery] string? createUser = null,
            [FromQuery] string? operatorUser = null,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var createTime = context.GetQueryDateTimeRangeNullable("createTime");
            var updateTime = context.GetQueryDateTimeRangeNullable("updateTime");
            var aftersalesBillRepo = context.RequestServices.GetRequiredService<IAftersalesBillRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await aftersalesBillRepo.QueryAsync(
                orderNumber, aftersalesNumber, businessType,
                userId, refundAmount, auditStatus,
                refundReason, sellerNote, createTime,
                updateTime, createUser, operatorUser,
                orderBy, desc, current,
                pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询售后单");

        routeGroup.MapPut("{id}/audit", async (HttpContext context,
            [FromRoute] Guid id,
            [FromBody] EditAftersalesBillAuditModel model) =>
        {
            var userId = context.GetBMUserId();
            var aftersalesBillRepo = context.RequestServices.GetRequiredService<IAftersalesBillRepository>();
            BMApiRsp r = await aftersalesBillRepo.AuditAsync(
                userId, id, model);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("审核售后单");
    }
}
