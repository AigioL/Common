using AigioL.Common.Net.ReverseProxy.Infrastructure.NameResolution;
using Microsoft.AspNetCore.Connections;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

#pragma warning disable IDE0290 // 使用主构造函数
namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Http;

/// <summary>
/// TCP 反射代理处理
/// </summary>
abstract class TcpReverseProxyHandler : ConnectionHandler
{
    readonly IDomainResolver domainResolver;
    readonly DnsEndPoint endPoint;
    readonly TimeSpan connectTimeout = TimeSpan.FromSeconds(10d);

    public TcpReverseProxyHandler(IDomainResolver domainResolver, DnsEndPoint endPoint)
    {
        this.domainResolver = domainResolver;
        this.endPoint = endPoint;
    }

    /// <summary>
    /// TCP 连接后
    /// </summary>
    public override async Task OnConnectedAsync(ConnectionContext context)
    {
        try
        {
            var cancellationToken = context.ConnectionClosed;
            using var connection = await CreateConnectionAsync(cancellationToken);
            var t1 = connection.CopyToAsync(context.Transport.Output, cancellationToken);
            var t2 = context.Transport.Input.CopyToAsync(connection, cancellationToken);
            await Task.WhenAny(t1, t2);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// 创建连接
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="AggregateException"></exception>
    async Task<Stream> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        var innerExceptions = new List<Exception>();
        var result = await domainResolver.ResolveAddressesAsync(endPoint.Host, cancellationToken: cancellationToken);
        if (result.Result.Records.Count != 0)
        {
            foreach (var it in result.Result.Records)
            {
                var socket = new Socket(it.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    using var timeoutTokenSource = new CancellationTokenSource(connectTimeout);
                    using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token);
                    await socket.ConnectAsync(it.Address, endPoint.Port, linkedTokenSource.Token);
                    return new NetworkStream(socket, ownsSocket: false);
                }
                catch (Exception ex)
                {
                    socket.Dispose();
                    cancellationToken.ThrowIfCancellationRequested();
                    innerExceptions.Add(ex);
                }
            }
        }
        throw new AggregateException($"TcpReverseProxyHandler 无法连接到 {endPoint.Host}:{endPoint.Port}.", innerExceptions);
    }
}
