using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers;

public static class UserOrderController
{
    public static void MapOrderingUserOrder(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ordering/userorder")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize);

        routeGroup.MapGet("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var userId = context.GetUserIdThrowIfNull();
            var repo = context.RequestServices.GetRequiredService<IOrderRepository>();
            var r = await GetOrderDetail(userId, repo, id, context.RequestAborted);
            return r;
        }).WithDescription("获取用户订单信息");
        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] long? orderNumber = null,
            [FromQuery] int? businessType = null,
            [FromQuery] string? note = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var status = context.GetQueryEnums<OrderStatus>("status");
            var paymentTime = context.GetQueryDateTimeRange("paymentTime");
            var creationTime = context.GetQueryDateTimeRange("creationTime");
            if (note == null)
            {
                if (context.Request.Query.TryGetValue("remarks", out var remarks) && !StringValues.IsNullOrEmpty(remarks))
                {
                    // 兼容旧数据结构
                    note = remarks;
                }
            }
            var r = await QueryAsync(
                context, orderNumber, status,
                paymentTime, businessType, note,
                creationTime, current, pageSize);
            return r;
        }).WithDescription("分页查询订单");
        routeGroup.MapGet("ExternalAccountInfo", async (HttpContext context,
            [FromQuery] string orderNumber,
            [FromQuery] string paymentNumber = "") =>
        {
            var r = await GetExternalAccountInfo(
                context, orderNumber, paymentNumber);
            return r;
        }).WithDescription("通过支付记录查询用户绑定外部平台信息");
        routeGroup.MapGet("count", async (HttpContext context,
            //[FromQuery] OrderStatus?[]? status = null,
            [FromQuery] int? businessType = null) =>
        {
            var status = context.GetQueryEnums<OrderStatus>("status");
            var r = await GetUserOrderCount(context, status, businessType);
            return r;
        }).WithDescription("通过条件获取用户订单数量");
    }

    /// <summary>
    /// 获取用户订单信息
    /// </summary>
    static async Task<ApiRsp<OrderDetailModel?>> GetOrderDetail(
        Guid userId,
        IOrderRepository repo,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await repo.GetOrderInfo(id, userId, cancellationToken);
        return result;
    }

    /// <summary>
    /// 分页查询订单
    /// </summary>
    /// <param name="context"></param>
    /// <param name="orderNumber">订单号</param>
    /// <param name="status">订单状态</param>
    /// <param name="paymentTime">支付时间</param>
    /// <param name="businessType">业务类型</param>
    /// <param name="note">订单备注</param>
    /// <param name="creationTime">创建时间</param>
    /// <param name="current">当前页码，页码从 1 开始，默认值：<see cref="IPagedModel.DefaultCurrent"/></param>
    /// <param name="pageSize">页大小，如果为 0 必定返回空集合，默认值：<see cref="IPagedModel.DefaultPageSize"/></param>
    /// <returns>分页表格查询结果数据</returns>
    static async Task<ApiRsp<PagedModel<OrderItemInfoModel>?>> QueryAsync(
        HttpContext context,
        long? orderNumber,
        OrderStatus[]? status,
        DateTimeOffset[]? paymentTime,
        int? businessType,
        string? note,
        DateTimeOffset[]? creationTime,
        int current,
        int pageSize)
    {
        // TODO: 分页查询订单
        throw new NotImplementedException("TODO: 分页查询订单");
        await Task.CompletedTask;
    }

    /// <summary>
    /// 通过支付记录查询用户绑定外部平台信息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="orderNumber">商家订单号</param>
    /// <param name="paymentNumber">支付平台订单号</param>
    /// <returns></returns>
    static async Task<ApiRsp<IEnumerable<ExternalLoginChannelWithNickName>?>> GetExternalAccountInfo(
        HttpContext context,
        string orderNumber,
        string paymentNumber)
    {
        // TODO: 分页查询订单
        throw new NotImplementedException("TODO: 分页查询订单");
        await Task.CompletedTask;
    }

    /// <summary>
    /// 通过条件获取用户订单数量
    /// </summary>
    /// <param name="context"></param>
    /// <param name="status">订单状态</param>
    /// <param name="businessType">订单业务类型</param>
    /// <returns></returns>
    static async Task<ApiRsp<int>> GetUserOrderCount(
        HttpContext context,
        OrderStatus[]? status,
        int? businessType)
    {
        // TODO: 分页查询订单
        throw new NotImplementedException("TODO: 分页查询订单");
        await Task.CompletedTask;
    }
}

#if DEBUG
[Obsolete("use ExternalLoginChannelWithNickName", true)]
public record ExternalAccountInfo(ExternalLoginChannel Channel, string NickName);
#endif