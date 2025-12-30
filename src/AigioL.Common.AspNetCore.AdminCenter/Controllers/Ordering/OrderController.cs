using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
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

        //routeGroup.MapGet("bizorder/{orderId}", async (HttpContext context,
        //    [FromRoute] string? orderId) =>
        //{
        //}).PermissionFilter(ControllerName, BMButtonType.Detail)
        //.WithDescription("获取通用订单的业务订单详情");

        routeGroup.MapGet("paymentservicestatus", async (HttpContext context) =>
        {
            var distributedCache = context.RequestServices.GetRequiredService<IDistributedCache>();
            var status = await CacheKeys.GetPaymentServiceStatus(distributedCache);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(status);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("获取支付服务状态");

        routeGroup.MapGet("paymentservicestatus/{enable}", async (HttpContext context,
            [FromRoute] bool enable) =>
        {
            var distributedCache = context.RequestServices.GetRequiredService<IDistributedCache>();
            await CacheKeys.SetPaymentServiceStatus(distributedCache, enable);
            BMApiRsp r = true;
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .RequireAuthorization(new AuthorizeAttribute()
        {
            AuthenticationSchemes = BMMinimalApis.BearerScheme,
            Roles = InfoController.RoleNameAdministrator,
        })
        .WithDescription("设置支付服务状态");

        //routeGroup.MapGet("orderinfo", async (HttpContext context,
        //    [FromQuery] string? id) =>
        //{
        //}).PermissionFilter(ControllerName, BMButtonType.Edit)
        //.WithDescription("获取通用订单详情");
    }
}
