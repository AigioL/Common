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
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize);

        routeGroup.MapGet("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var r = await GetOrderPaymentInfo(context, id);
            return r;
        }).WithDescription("获取订单支付信息")
        .AllowAnonymous(); // 允许匿名访问

    }

    /// <summary>
    /// 获取订单支付信息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    static async Task<ApiRsp<OrderPayInfoModel?>> GetOrderPaymentInfo(
        HttpContext context,
        Guid id)
    {
        var repo = context.RequestServices.GetRequiredService<IOrderRepository>();
        var result = await repo.GetOrderPaymentInfo(id);
        return result;
    }
}
