using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Basics;

/// <summary>
/// Clash 网络代理管理
/// </summary>
public static partial class ClashProxyController
{
    const string ControllerName = ControllerConstants.ClashProxy;

    public static void MapClashProxyController(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/basics/clashproxy",
        int valueCountLimit = 100_0000)
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("Clash 网络代理管理");

        routeGroup.MapGet("proxies/all", (HttpContext context,
            [FromQuery] string[]? p = null,
            [FromQuery] string? u = null,
            [FromQuery] string? s = null) =>
        {
            return Results.Content("TODO");
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("根据 Clash ApiUrl 与 多个端口号获取当前正在使用的节点名组与所有节点");

        routeGroup.MapPost("proxies/speedtest/{clashProxiesName}/{controllerApiUrl?}/{clashSecret?}", (HttpContext context,
            [FromRoute] string clashProxiesName,
            [FromRoute] string? controllerApiUrl = null,
            [FromRoute] string? clashSecret = null) =>
        {
            return Results.Content("TODO");
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("节点测速");

        routeGroup.MapPut("proxies/update/{controllerApiUrl?}/{clashSecret?}", (HttpContext context,
            [FromRoute] string? controllerApiUrl = null,
            [FromRoute] string? clashSecret = null) =>
        {
            return Results.Content("TODO");
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithFormOptions(valueCountLimit: valueCountLimit) // [RequestFormLimits(ValueCountLimit = 1000000)]
        .WithDescription("更新配置文件");
    }
}
