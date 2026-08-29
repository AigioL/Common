using AigioL.Common.Net.NameResolution.Abstractions;
using AigioL.Common.Net.NameResolution.Models;
using AigioL.Common.Net.NameResolution.Services;
using System.Net;
using System.Net.Sockets;

#pragma warning disable IDE0290 // 使用主构造函数
namespace AigioL.Common.Net.ReverseProxy.Infrastructure.NameResolution;

/// <summary>
/// 固定配置的 DNS 记录解析，作为配置无效时或解析的地址不正确等非正常状态时的备用回退尝试
/// </summary>
sealed partial class FixedServersDnsResolver : IAsyncDisposable, IDisposable
{
    const string traceId = "FixedServers";
    bool disposedValue;
    DnsResolverWrapper? resolver;

    [ActivatorUtilitiesConstructor]
    public FixedServersDnsResolver(ILoggerFactory loggerFactory) : this(loggerFactory, null)
    {
    }

    public FixedServersDnsResolver(ILoggerFactory loggerFactory, DnsResolverOptions? options)
    {
        resolver = new(loggerFactory.CreateLogger<DnsResolverWrapper>(), options ?? CreateOptionsIPv4(), traceId);
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        resolver?.Dispose();
        resolver = null;
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
        disposedValue = true;

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="IDnsResolver.ResolveAddressesAsync(string, AddressFamily, CancellationToken)"/>
    public Task<DnsResultWrapper<AddressRecord>> ResolveAddressesAsync(string hostName, AddressFamily addressFamily = AddressFamily.Unspecified, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposedValue || resolver == null, this);
        return resolver.ResolveAddressesAsync(hostName, addressFamily, cancellationToken);
    }
}
