using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
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

    static async Task<ApiRsp<AftersalesBillDetailModel?>> CreateAftersalesBill(
        HttpContext context,
        AftersalesBillAddDto m)
    {
        throw new NotImplementedException("TODO: 实现创建售后单逻辑");
        await Task.CompletedTask;
    }
}
