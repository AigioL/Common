using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
            var userId = context.GetUserIdThrowIfNull();
            var repo = context.RequestServices.GetRequiredService<IOrderRepository>();
            var r = await QueryAsync(
                userId, repo, orderNumber, status,
                paymentTime, businessType, note,
                creationTime, current, pageSize,
                context.RequestAborted);
            return r;
        }).WithDescription("分页查询订单");
        routeGroup.MapGet("ExternalAccountInfo", async (HttpContext context,
            [FromQuery] string orderNumber,
            [FromQuery] string paymentNumber = "") =>
        {
            var memoryCache = context.RequestServices.GetRequiredService<IMemoryCache>();
            var userId = context.GetUserIdThrowIfNull();
            var repo = context.RequestServices.GetRequiredService<IOrderRepository>();
            var r = await GetExternalAccountInfo(
                memoryCache, userId, repo,
                orderNumber, paymentNumber, context.RequestAborted);
            return r;
        }).WithDescription("通过支付记录查询用户绑定外部平台信息");
        routeGroup.MapGet("count", async (HttpContext context,
            //[FromQuery] OrderStatus?[]? status = null,
            [FromQuery] int? businessType = null) =>
        {
            var userId = context.GetUserIdThrowIfNull();
            var repo = context.RequestServices.GetRequiredService<IOrderRepository>();
            var status = context.GetQueryEnums<OrderStatus>("status");
            var r = await GetUserOrderCount(
                userId, repo, status,
                businessType, context.RequestAborted);
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
    /// <param name="userId"></param>
    /// <param name="repo"></param>
    /// <param name="context"></param>
    /// <param name="orderNumber">订单号</param>
    /// <param name="status">订单状态</param>
    /// <param name="paymentTime">支付时间</param>
    /// <param name="businessType">业务类型</param>
    /// <param name="note">订单备注</param>
    /// <param name="creationTime">创建时间</param>
    /// <param name="current">当前页码，页码从 1 开始，默认值：<see cref="IPagedModel.DefaultCurrent"/></param>
    /// <param name="pageSize">页大小，如果为 0 必定返回空集合，默认值：<see cref="IPagedModel.DefaultPageSize"/></param>
    /// <param name="cancellationToken"></param>
    /// <returns>分页表格查询结果数据</returns>
    static async Task<ApiRsp<PagedModel<OrderItemInfoModel>?>> QueryAsync(
        Guid userId,
        IOrderRepository repo,
        long? orderNumber,
        OrderStatus[]? status,
        DateTimeOffset[]? paymentTime,
        int? businessType,
        string? note,
        DateTimeOffset[]? creationTime,
        int current,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var r = await repo.QueryUserOrderListAsync(
            userId, orderNumber, status,
            paymentTime, businessType, note,
            creationTime, current, pageSize,
            cancellationToken);
        return r;
    }

    /// <summary>
    /// 通过支付记录查询用户绑定外部平台信息
    /// </summary>
    /// <param name="memoryCache"></param>
    /// <param name="userId"></param>
    /// <param name="repo"></param>
    /// <param name="orderNumber">商家订单号</param>
    /// <param name="paymentNumber">支付平台订单号</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    static async Task<ApiRsp<ExternalLoginChannelWithNickName[]?>> GetExternalAccountInfo(
        IMemoryCache memoryCache,
        Guid userId,
        IOrderRepository repo,
        string orderNumber,
        string paymentNumber,
        CancellationToken cancellationToken = default)
    {
        if (IsUserRateLimited(memoryCache, userId))
        {
            return "请求过于频繁";
        }
        var accounts = await repo.GetExternalAccountInfoAsync(
            orderNumber, paymentNumber,
            cancellationToken);
        var r = accounts
            ?.Select(a => new ExternalLoginChannelWithNickName(a.Channel, StarredMasking(a.NickName)))
            ?.ToArray();
        return r;
    }

    /// <summary>
    /// 通过条件获取用户订单数量
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="repo"></param>
    /// <param name="context"></param>
    /// <param name="status">订单状态</param>
    /// <param name="businessType">订单业务类型</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    static async Task<ApiRsp<int>> GetUserOrderCount(
        Guid userId,
        IOrderRepository repo,
        OrderStatus[]? status,
        int? businessType,
        CancellationToken cancellationToken = default)
    {
        var r = await repo.GetUserOrderCountAsync(
            userId, status, businessType,
            cancellationToken);
        return r;
    }

    /// <summary>
    /// 星号打码
    /// </summary>
    static string StarredMasking(string? s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return s;
        }

        const int beginning = 2;
        const int ending = 1;
        const int star = 3;
        var r = string.Create(beginning + star + ending, s, (buffer, value) =>
        {
            buffer.Fill('*');
            value.AsSpan(0, Math.Min(beginning, value.Length)).CopyTo(buffer);
            value.AsSpan(value.Length - ending).CopyTo(buffer[^Math.Min(ending, value.Length)..]);
        });
        return r;
    }

    /// <summary>
    /// 用户请求频率是否限制（只允许 5 分钟内调用一次）
    /// </summary>
    static bool IsUserRateLimited(IMemoryCache memoryCache, Guid userId, long minutes = 5)
    {
        var key = $"UserRateLimited_{userId}";
        if (memoryCache.Get(key) != null)
        {
            return true;
        }
        memoryCache.Set(key, true, TimeSpan.FromMinutes(minutes));
        return false;
    }
}

#if DEBUG
[Obsolete("use ExternalLoginChannelWithNickName", true)]
public record ExternalAccountInfo(ExternalLoginChannel Channel, string NickName);
#endif