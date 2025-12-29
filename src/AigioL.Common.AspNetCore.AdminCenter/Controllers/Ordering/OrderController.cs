using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Ordering.Models.OrderTableItem;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Ordering;

/// <summary>
/// 通用订单管理
/// </summary>
public static partial class OrderController
{
    const string ControllerName = ControllerConstants.Order;

    public static void MapOrder(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/ordering/orders")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("通用订单管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] string? id,
            [FromQuery] string? orderNumber,
            [FromQuery] OrderType? type,
            [FromQuery] DevicePlatform2? source,
            [FromQuery] OrderStatus[]? status,
            [FromQuery] Guid? userId,
            [FromQuery] PaymentType? paymentType,
            [FromQuery] int? businessType,
            [FromQuery] string? note,
            [FromQuery] Guid? agreementId,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var createTime = context.GetQueryDateTimeRangeNullable("createTime");
            var paymentTime = context.GetQueryDateTimeRangeNullable("paymentTime");
            var orderRepo = context.RequestServices.GetRequiredService<IOrderRepository>();
            BMApiRsp<PagedModelEx<TableItemM, decimal>?> r = await orderRepo.QueryAsync(
                id, orderNumber, type,
                source, status, userId,
                businessType, note, paymentTime,
                paymentType, createTime, agreementId,
                orderBy, desc, current,
                pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询通用订单");
    }
}
