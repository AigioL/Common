using AigioL.Common.Net.NameResolution.Abstractions;
using AigioL.Common.Net.NameResolution.Models;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using static AigioL.Common.Net.NameResolution.Services.LoggerMessages;

namespace AigioL.Common.Net.NameResolution.Services;

/// <summary>
/// DNS 记录解析，可选地使用调用方指定的 DNS 服务器集
/// </summary>
public sealed class DnsResolverWrapper : IDnsResolver, IAsyncDisposable, IDisposable
{
    bool disposedValue;
    DnsResolver? resolver;
    IList<IPEndPoint>? servers;
    readonly ILogger logger;
    readonly string? traceId;

    /// <summary>
    /// 使用指定的 <see cref="DnsResolverOptions"/> 初始化 <see cref="DnsResolverWrapper"/> 实例
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="options">要使用的 <see cref="DnsResolverOptions"/> 实例</param>
    /// <param name="traceId"></param>
    public DnsResolverWrapper(ILogger<DnsResolverWrapper> logger, DnsResolverOptions? options = null, string? traceId = null)
    {
        this.logger = logger;
        this.traceId = traceId;
        servers = options?.Servers;
        if (options != null)
        {
            resolver = new(options);
        }
    }

    /// <summary>
    /// 使用指定的 DNS 服务器集初始化 <see cref="DnsResolverWrapper"/> 实例
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="servers">要使用的 DNS 服务器集</param>
    /// <param name="traceId"></param>
    public DnsResolverWrapper(ILogger<DnsResolverWrapper> logger, IList<IPEndPoint>? servers, string? traceId = null) : this(logger, servers == null ? null : new DnsResolverOptions
    {
        Servers = servers,
    }, traceId)
    {
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        resolver?.Dispose();
        resolver = null;
        servers = null;
        disposedValue = true;

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="IAsyncDisposable.DisposeAsync"/>
    public async ValueTask DisposeAsync()
    {
        if (resolver != null)
        {
            await resolver.DisposeAsync().ConfigureAwait(false);
        }
        resolver = null;
        servers = null;
        disposedValue = true;

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 比较当前实例的 DNS 服务器集与指定的 DNS 服务器集是否相等
    /// </summary>
    /// <param name="servers">要比较的 DNS 服务器集</param>
    /// <returns>如果两个 DNS 服务器集相等，则返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    public bool EqualsServers(IList<IPEndPoint>? servers)
    {
        if (this.servers == null)
        {
            return servers == null;
        }
        else if (servers == null)
        {
            return false;
        }

        if (servers.Count != this.servers.Count)
        {
            return false;
        }

        for (int i = 0; i < servers.Count; i++)
        {
            var l = this.servers[i];
            var r = servers[i];
            if (!l.Equals(r))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 比较当前实例的 DNS 服务器集与指定的 <see cref="DnsResolverWrapper"/> 实例的 DNS 服务器集是否相等
    /// </summary>
    /// <param name="dnsResolver">要比较的 <see cref="DnsResolverWrapper"/> 实例</param>
    /// <returns>如果两个 DNS 服务器集相等，则返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    public bool EqualsServers(DnsResolverWrapper dnsResolver)
    {
        var servers = dnsResolver.servers;
        if (servers == null)
        {
            return false;
        }
        return EqualsServers(servers);
    }

    public bool HasServers => servers != null && servers.Count > 0;

    /// <inheritdoc/>
    public async Task<DnsResultWrapper<AddressRecord>> ResolveAddressesAsync(string hostName, AddressFamily addressFamily = AddressFamily.Unspecified, CancellationToken cancellationToken = default)
    {
        try
        {
#if NET11_0_OR_GREATER
            ObjectDisposedException.ThrowIf(disposedValue, this);
            if (resolver == null)
            {
                // 没有配置任何 DoH 服务器
                return DnsResponseCode.NotImplemented;
            }
            else
            {
                var timestamp = Stopwatch.GetTimestamp();
                var r = await resolver.ResolveAddressesAsync(hostName, addressFamily, cancellationToken);
                return r.ToWrapper(DnsResultSourceType.DnsServers, traceId, Stopwatch.GetElapsedTime(timestamp));
            }
#else
        throw new NotImplementedException();
#endif
        }
        catch (OperationCanceledException)
        {
            return DnsResponseCode.ServerFailure;
        }
        catch (Exception ex)
        {
            LogErrorDnsResolveAddresses(logger, ex, hostName, addressFamily);
            return DnsResponseCode.ServerFailure;
        }
    }
}

static partial class LoggerMessages
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "DNS 解析主机名失败，hostName: {hostName}, addressFamily: {addressFamily}")]
    internal static partial void LogErrorDnsResolveAddresses(ILogger logger, Exception? exception, string? hostName, AddressFamily addressFamily);
}