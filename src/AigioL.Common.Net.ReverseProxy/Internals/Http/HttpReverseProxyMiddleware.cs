using Microsoft.AspNetCore.Http;

namespace AigioL.Common.Net.ReverseProxy.Internals.Http;

/// <summary>
/// HTTP 反向代理中间件
/// </summary>
sealed partial class HttpReverseProxyMiddleware
{
    /// <summary>
    /// 处理请求
    /// </summary>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {

    }
}
