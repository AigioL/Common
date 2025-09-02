using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Controllers;

/// <summary>
/// 活跃用户匿名统计
/// </summary>
public static class ActiveUsersController
{
    public static void MapAnalyticsActiveUsers(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "analysis/activeusers")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] ActiveUserRecordModel m) =>
        {
            var r = await PostAsync(context, m);
            return r;
        });

    }

    static async Task<ApiRsp> PostAsync(HttpContext context, ActiveUserRecordModel m)
    {
        throw new NotImplementedException("TODO: 实现日志处理逻辑");
        await Task.CompletedTask;
    }
}
