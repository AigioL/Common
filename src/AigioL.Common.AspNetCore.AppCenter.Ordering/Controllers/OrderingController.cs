using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers;

public static class OrderingController
{
    public static void MapOrdering(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ordering")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous(); // 允许匿名访问

        routeGroup.MapGet("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var repo = context.RequestServices.GetRequiredService<IOrderRepository>();
            var r = await GetOrderPaymentInfo(repo, id, context.RequestAborted);
            return r;
        }).WithDescription("获取订单支付信息");

    }

    /// <summary>
    /// 获取订单支付信息
    /// </summary>
    static async Task<ApiRsp<OrderPayInfoModel?>> GetOrderPaymentInfo(
        IOrderRepository repo,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await repo.GetOrderPaymentInfo(id, cancellationToken: cancellationToken);
        return result;
    }
}
