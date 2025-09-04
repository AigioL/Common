using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.AppCenter;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Controllers;

/// <summary>
/// https://learn.microsoft.com/zh-cn/appcenter/quickstarts
/// </summary>
public static class LogsController
{
    const string AppSecretHeader = "App-Secret";
    const string InstallIdHeader = "Install-ID";
    const int MaxRetryCount = 5;
    const int LockExpirySeconds = 3;
    const int RetryDelaySeconds = 1;

    public static void MapAnalyticsLogs(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "analysis/logs")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AnalysisLogContainerModel m) =>
        {
            var r = await PostAsync(context, m);
            return r;
        }).WithDescription("Visual Studio App Center 分析日志上传");

    }

    /// <summary>
    /// Visual Studio App Center 于 2025 年 3 月 31 日停用，但分析和诊断功能除外，这些功能将继续受支持，直到 2026 年 6 月 30 日。 
    /// <para>更改 Visual Studio App Center 客户端 SDK 将日志发送地址设为此接口替代停用的 App Center 服务</para>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="m"></param>
    /// <returns></returns>
    static async Task<ApiRsp> PostAsync(HttpContext context, AnalysisLogContainerModel m)
    {
        return "TODO: 实现日志处理逻辑";

        if (m == null || m.Logs == null || m.Logs.Count == 0)
        {
            return HttpStatusCode.BadRequest;
        }

        var appSecret = context.Request.Headers[AppSecretHeader];
        var installId = context.Request.Headers[InstallIdHeader];
        if (StringValues.IsNullOrEmpty(appSecret) || StringValues.IsNullOrEmpty(installId))
        {
            return HttpStatusCode.BadRequest;
        }

        // TODO: 目前 Windows 版 App Center SDK 不兼容 AOT 与裁剪
        throw new NotImplementedException("TODO: 实现日志处理逻辑");
        await Task.CompletedTask;
    }
}
