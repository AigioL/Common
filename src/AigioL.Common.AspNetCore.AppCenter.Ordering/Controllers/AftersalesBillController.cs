using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers;

public static class AftersalesBillController
{
    public static void MapOrderingAftersalesBill(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ordering/aftersalesbill")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize);

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AftersalesBillAddDto m) =>
        {
            var userId = context.GetUserIdThrowIfNull();
            var repo = context.RequestServices.GetRequiredService<IAftersalesBillRepository>();
            var connection = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
            var r = await CreateAftersalesBill(userId, repo, connection, m, context.RequestAborted);
            return r;
        }).WithDescription("创建售后单");

    }

    /// <summary>
    /// 创建售后单
    /// </summary>
    static async Task<ApiRsp<AftersalesBillDetailModel?>> CreateAftersalesBill(
        Guid userId,
        IAftersalesBillRepository repo,
        IConnectionMultiplexer connection,
        AftersalesBillAddDto m,
        CancellationToken cancellationToken = default)
    {
        var redisDb = connection.GetDatabase(CacheKeys.RedisMessagingDb);
        var result = await repo.CreateAftersalesBill(m.OrderId, m.RefundReason, userId, cancellationToken);
        if (!result.IsSuccess())
        {
            var error = result.Message;
            ArgumentNullException.ThrowIfNull(error);
            return error;
        }
        else
        {
            var aftersalesBill = result.Content.aftersalesBill;
            ArgumentNullException.ThrowIfNull(aftersalesBill);
            var order = result.Content.order;
            ArgumentNullException.ThrowIfNull(order);

            // 通知业务订单要中止业务
            var channel = CacheKeys.GetOrderUserRequestRefundMessageQueueKeyByBusinessType(order.BusinessTypeId);
            await redisDb.ListRightPushAsync(channel, m.OrderId.ToString());

            return aftersalesBill;
        }
    }
}
