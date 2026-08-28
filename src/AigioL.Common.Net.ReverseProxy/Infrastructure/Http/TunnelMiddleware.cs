using AigioL.Common.Net.NameResolution.Abstractions;
using AigioL.Common.Net.ReverseProxy.Infrastructure.Configuration;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;

#pragma warning disable IDE0290 // 使用主构造函数
namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Http;

/// <summary>
/// 隧道中间件
/// </summary>
sealed class TunnelMiddleware
{
    readonly IReverseProxyConfig reverseProxyConfig;
    readonly IDnsResolver dnsResolver;
    readonly TimeSpan connectTimeout = TimeSpan.FromSeconds(10d);

    public TunnelMiddleware(
        IReverseProxyConfig reverseProxyConfig,
        IDnsResolver dnsResolver)
    {
        this.reverseProxyConfig = reverseProxyConfig;
        this.dnsResolver = dnsResolver;
    }

    /// <summary>
    /// 处理连接
    /// </summary>
    public async Task InvokeAsync(ConnectionDelegate next, ConnectionContext context)
    {
        try
        {
            var proxyFeature = context.Features.Get<IHttpProxyFeature>();
            if (proxyFeature == null || // 非代理
                proxyFeature.ProxyProtocol != ProxyProtocol.TunnelProxy || // 非隧道代理
                context.Features.Get<ITlsConnectionFeature>() != null) // 经过隧道的 HTTPS
            {
                await next(context);
            }
            else
            {
                var transport = context.Features.Get<IConnectionTransportFeature>()?.Transport;
                if (transport != null)
                {
                    var cancellationToken = context.ConnectionClosed;
                    using var connection = await CreateConnectionAsync(proxyFeature.ProxyHost, cancellationToken);

                    var t1 = connection.CopyToAsync(transport.Output, cancellationToken);
                    var t2 = transport.Input.CopyToAsync(connection, cancellationToken);
                    await Task.WhenAny(t1, t2);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// 创建连接
    /// </summary>
    async Task<Stream> CreateConnectionAsync(HostString host, CancellationToken cancellationToken)
    {
        var innerExceptions = new List<Exception>();
        await foreach (var endPoint in GetUpstreamEndPointsAsync(host, cancellationToken))
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try
            {
                using var timeoutTokenSource = new CancellationTokenSource(connectTimeout);
                using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token);
                await socket.ConnectAsync(endPoint, linkedTokenSource.Token);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch (Exception ex)
            {
                socket.Dispose();
                cancellationToken.ThrowIfCancellationRequested();
                innerExceptions.Add(ex);
            }
        }
        throw new AggregateException($"TunnelMiddleware 无法连接到 {host}", innerExceptions);
    }

    /// <summary>
    /// 获取连接端点
    /// </summary>
    async IAsyncEnumerable<EndPoint> GetUpstreamEndPointsAsync(HostString host, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var port = host.Port ?? HTTPS_PORT;

        if (IPAddress2.TryParse(host.Host, out var address))
        {
            yield return new IPEndPoint(address, port); // 如果 HOST 是 IP 地址，则直接返回 IPEndPoint
        }
        else if (!reverseProxyConfig.IsMatch(host))
        {
            yield return new DnsEndPoint(host.Host, port); // 如果 HOST 不在反向代理配置中，则直接返回 DnsEndPoint
        }
        else
        {
            // 当 HOST 在反向代理配置中时，使用 DNS 解析器解析 HOST 的 IP 地址
            var result = await dnsResolver.ResolveAddressesAsync(host.Host, cancellationToken: cancellationToken);
            if (result.Result.Records.Count != 0)
            {
                foreach (var it in result.Result.Records)
                {
                    yield return new IPEndPoint(it.Address, port);
                }
            }
        }
    }
}
