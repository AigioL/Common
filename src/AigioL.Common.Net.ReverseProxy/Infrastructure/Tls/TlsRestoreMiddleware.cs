using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Tls;

/// <summary>
/// TLS 恢复中间件
/// </summary>
static class TlsRestoreMiddleware
{
    /// <summary>
    /// 执行 TLS 恢复中间件
    /// </summary>
    public static async Task InvokeAsync(ConnectionDelegate next, ConnectionContext context)
    {
        var tlsConnFeature = context.Features.Get<ITlsConnectionFeature>();
        if (tlsConnFeature is EmptyTlsConnectionFeature)
        {
            // 移除 TLS 入侵
            context.Features.Set<ITlsConnectionFeature>(null);
        }
        await next(context);
    }
}
