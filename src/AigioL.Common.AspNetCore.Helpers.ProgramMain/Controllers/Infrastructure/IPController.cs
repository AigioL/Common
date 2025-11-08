using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace AigioL.Common.AspNetCore.Helpers.ProgramMain.Controllers.Infrastructure;

/// <summary>
/// IP 地址测试接口
/// </summary>
public static class IPController
{
    /// <summary>
    /// 测试 IPv6 获取
    /// </summary>
    /// <param name="b"></param>
    /// <param name="pattern"></param>
    public static void MapGetIpV6(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "{projId}/ip/v6")
    {
        pattern = ProgramHelper.GetEndpointPattern(pattern);

        b.MapGet(pattern, (HttpContext context) =>
        {
            var ip = context.Connection.RemoteIpAddress;
            if (ip != null && ip.AddressFamily == AddressFamily.InterNetworkV6)
                return Results.Ok();
            return Results.BadRequest();
        })
        .AllowAnonymous()
        .WithDescription("测试 IPv6 获取");
    }

    /// <summary>
    /// 测试 IP 地址获取
    /// </summary>
    /// <param name="b"></param>
    /// <param name="pattern"></param>
    public static void MapGetIpVal(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "{projId}/ip/val")
    {
        pattern = ProgramHelper.GetEndpointPattern(pattern);

        b.MapGet(pattern, (HttpContext context) =>
        {
            var ip = context.Connection.RemoteIpAddress;
            return Results.Content(ip?.ToString());
        })
        .AllowAnonymous()
        .WithDescription("测试 IP 地址获取");
    }
}
