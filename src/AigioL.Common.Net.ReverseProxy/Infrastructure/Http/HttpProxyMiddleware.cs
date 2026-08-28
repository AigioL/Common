using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using System.Buffers;
using System.IO.Pipelines;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Http;

/// <summary>
/// 正向代理中间件
/// </summary>
sealed class HttpProxyMiddleware
{
    readonly HttpParser<HttpRequestHandler> httpParser = new();

    /// <summary>
    /// 处理连接
    /// </summary>
    public async Task InvokeAsync(ConnectionDelegate next, ConnectionContext context)
    {
        try
        {
            var input = context.Transport.Input;
            var output = context.Transport.Output;
            var request = new HttpRequestHandler();

            while (context.ConnectionClosed.IsCancellationRequested == false)
            {
                try
                {
                    var result = await input.ReadAsync(context.ConnectionClosed);
                    if (result.IsCanceled)
                    {
                        break;
                    }

                    if (ParseRequest(result, request, out var consumed))
                    {
                        if (request.ProxyProtocol == ProxyProtocol.TunnelProxy)
                        {
                            input.AdvanceTo(consumed);
                            var http200 = "HTTP/1.1 200 Connection Established\r\n\r\n"u8;
                            output.Write(http200);
                            await output.FlushAsync(context.ConnectionClosed);
                        }
                        else
                        {
                            input.AdvanceTo(result.Buffer.Start);
                        }

                        context.Features.Set<IHttpProxyFeature>(request);
                        await next(context);

                        break;
                    }
                    else
                    {
                        input.AdvanceTo(result.Buffer.Start, result.Buffer.End);
                    }

                    if (result.IsCompleted)
                    {
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception)
                {
                    var http400 = "HTTP/1.1 400 Bad Request\r\n\r\n"u8;
                    output.Write(http400);
                    await output.FlushAsync(context.ConnectionClosed);
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// 解析 HTTP 请求
    /// </summary>
    bool ParseRequest(ReadResult result, HttpRequestHandler request, out SequencePosition consumed)
    {
        var reader = new SequenceReader<byte>(result.Buffer);
        if (httpParser.ParseRequestLine(request, ref reader) &&
            httpParser.ParseHeaders(request, ref reader))
        {
            consumed = reader.Position;
            return true;
        }
        else
        {
            consumed = default;
            return false;
        }
    }
}
