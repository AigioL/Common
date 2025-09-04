using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers;

public static class AftersalesBillController
{
    public static void MapOrderingAftersalesBill(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ordering/aftersalesbill")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.MSApiControllerBaseAuthorize);

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AftersalesBillAddDto m) =>
        {
            var r = await CreateAftersalesBill(context, m);
            return r;
        }).WithDescription("创建售后单");

    }

    /// <summary>
    /// 创建售后单
    /// </summary>
    /// <param name="context"></param>
    /// <param name="m"></param>
    /// <returns></returns>
    static async Task<ApiRsp<AftersalesBillDetailModel?>> CreateAftersalesBill(
        HttpContext context,
        AftersalesBillAddDto m)
    {
        var userId = context.GetUserIdThrowIfNull();
        var repo = context.RequestServices.GetRequiredService<IAftersalesBillRepository>();
        var result = await repo.CreateAftersalesBill(m.OrderId, m.RefundReason, userId);
        return result;
    }
}
