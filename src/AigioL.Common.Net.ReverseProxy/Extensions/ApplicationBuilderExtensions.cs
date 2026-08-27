using AigioL.Common.Net.ReverseProxy.Internals.Http;
using AigioL.Common.Net.ReverseProxy.Internals.Logging;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Builder;

static partial class ApplicationBuilderExtensions
{
    /// <summary>
    /// 使用 HTTP 代理 PAC 中间件
    /// </summary>
    public static IApplicationBuilder UseHttpProxyPac(this IApplicationBuilder app)
    {
        var middleware = app.ApplicationServices.GetRequiredService<HttpProxyPacMiddleware>();
        return app.Use(next => context => middleware.InvokeAsync(context, next));
    }

    /// <summary>
    /// 使用本地 HTTP 代理策略中间件
    /// </summary>
    public static IApplicationBuilder UseHttpLocalRequest(this IApplicationBuilder app)
    {
        var middleware = app.ApplicationServices.GetRequiredService<HttpLocalRequestMiddleware>();
        return app.Use(next => context => middleware.InvokeAsync(context, next));
    }

    /// <summary>
    /// 使用请求日志中间件
    /// </summary>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        var middleware = app.ApplicationServices.GetRequiredService<RequestLoggingMiddleware>();
        return app.Use(next => context => middleware.InvokeAsync(context, next));
    }

    /// <summary>
    /// 禁用请求日志中间件
    /// </summary>
    public static IApplicationBuilder DisableRequestLogging(this IApplicationBuilder app) => app.Use(next => context =>
    {
        var loggingFeature = context.Features.Get<IRequestLoggingFeature>();
        loggingFeature?.Enable = false;
        return next(context);
    });

    /// <summary>
    /// 使用反向代理中间件
    /// </summary>
    public static IApplicationBuilder UseHttpReverseProxy(this IApplicationBuilder app)
    {
        var middleware = app.ApplicationServices.GetRequiredService<HttpReverseProxyMiddleware>();
        return app.Use(next => context => middleware.InvokeAsync(context, next));
    }
}