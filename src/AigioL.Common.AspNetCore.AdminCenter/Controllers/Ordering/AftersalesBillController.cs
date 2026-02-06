using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;
using System.Text;
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
            [FromQuery] DateTimeOffset?[]? createTime = null,
            [FromQuery] DateTimeOffset?[]? updateTime = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
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

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AftersalesBillAddModel m,
            [FromQuery] decimal? refundAmount = null) =>
        {
            var aftersalesBillRepo = context.RequestServices.GetRequiredService<IAftersalesBillRepository>();
            var rabbitmqConn = context.RequestServices.GetRequiredService<IConnection>();
            var result = await aftersalesBillRepo.CreateAftersalesBill(m.OrderId, m.RefundReason, null, refundAmount, context.RequestAborted);
            if (!result.IsSuccess())
            {
                var error = result.Message;
                ArgumentNullException.ThrowIfNull(error);
                return error;
            }

            var aftersalesBillDetailModel = result.Content.aftersalesBillDetailModel;
            ArgumentNullException.ThrowIfNull(aftersalesBillDetailModel);
            var order = result.Content.order;
            ArgumentNullException.ThrowIfNull(order);

            // 通知业务订单要中止业务
            var routingKey = CacheKeys.GetOrderUserRequestRefundMessageQueueKeyByBusinessType(order.BusinessTypeId);
            using var channel = await rabbitmqConn.CreateChannelAsync(cancellationToken: context.RequestAborted);
            await channel.BasicPublishAsync("", routingKey, Encoding.UTF8.GetBytes(m.OrderId), context.RequestAborted);

            BMApiRsp r = true;
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("添加售后单");

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
