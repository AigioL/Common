using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers;

public static class UserOrderController
{
    public static void MapOrderingUserOrder(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ordering/userorder")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.MSApiControllerBaseAuthorize);

        routeGroup.MapGet("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var r = await GetOrderDetail(context, id);
            return r;
        }).WithDescription("获取用户订单信息");

    }

    static async Task<ApiRsp<OrderDetailModel?>> GetOrderDetail(
        HttpContext context,
        Guid id)
    {
        throw new NotImplementedException("TODO: 实现获取用户订单信息逻辑");
        await Task.CompletedTask;
    }
}
