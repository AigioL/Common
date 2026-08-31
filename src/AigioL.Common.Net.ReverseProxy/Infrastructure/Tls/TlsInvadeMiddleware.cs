using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using System.Buffers;
using System.IO.Pipelines;
using static AigioL.Common.Net.ReverseProxy.Infrastructure.Tls.LoggerMessages;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Tls;

/// <summary>
/// TLS 入侵中间件
/// </summary>
static class TlsInvadeMiddleware
{
    /// <summary>
    /// 执行 TLS 入侵中间件
    /// </summary>
    public static async Task InvokeAsync(ConnectionDelegate next, ConnectionContext context)
    {
        var isTlsConn = await IsTlsConnectionAsync(context);
        if (!isTlsConn) // 连接不是 TLS
        {
            var tlsConnFeature = context.Features.Get<ITlsConnectionFeature>();
            if (tlsConnFeature == null) // 没有任何 TLS 中间件执行过
            {
                // 设置空的 ITlsConnectionFeature，迫使 HTTPS 中间件跳过自身的工作
                context.Features.Set<ITlsConnectionFeature>(EmptyTlsConnectionFeature.Instance);
            }
        }
        await next(context);
    }

    /// <summary>
    /// 是否为 TLS 协议
    /// </summary>
    internal static async Task<bool> IsTlsConnectionAsync(ConnectionContext context)
    {
        try
        {
            var result = await context.Transport.Input.ReadAtLeastAsync(2, context.ConnectionClosed);
            var isTlsConn = IsTlsProtocol(result);
            context.Transport.Input.AdvanceTo(result.Buffer.Start);
            return isTlsConn;
        }
        catch (Exception ex)
        {
            var ctx = context.GetHttpContext();
            if (ctx != null)
            {
                var loggerFactory = ctx.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger(nameof(TlsInvadeMiddleware));
                LogErrorIsTlsConnection(logger, ex, ctx.Request.GetDisplayUrl(), ctx.Request.Method);
            }
            return false;
        }
    }

    static bool IsTlsProtocol(ReadResult result)
    {
        var reader = new SequenceReader<byte>(result.Buffer);
        var isTls = reader.TryRead(out var firstByte) &&
            reader.TryRead(out var nextByte) &&
            firstByte == 0x16 &&
            nextByte == 0x3;
        return isTls;
    }
}

static partial class LoggerMessages
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "检测是否为 TLS 协议时发生错误, method: {method}, url: {url}")]
    internal static partial void LogErrorIsTlsConnection(ILogger logger, Exception exception, string? url, string? method);
}