using AigioL.Common.Net.NameResolution;
using AigioL.Common.Net.NameResolution.Models;
using AigioL.Common.Net.NameResolution.Services;
using AigioL.Common.Net.ReverseProxy.Infrastructure.Configuration;
using AigioL.Common.Net.ReverseProxy.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;
using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.NameResolution;

sealed class DomainResolver : IDomainResolver
{
    bool disposedValue;
    DnsResolver? resolver6;
    DnsComposeResolver? dnsResolver;
    readonly IReverseProxyConfig reverseProxyConfig;
    readonly FixedServersDnsResolver fixedServersDnsResolver;
    IMemoryCache memoryCache;
    readonly IServiceProvider serviceProvider;
    readonly ILoggerFactory loggerFactory;
    readonly Lock lockClearCache = new();

    public DnsComposeResolver DnsCompose
    {
        get
        {
            ObjectDisposedException.ThrowIf(disposedValue || dnsResolver == null, this);
            return dnsResolver;
        }
    }

    public DomainResolver(ILoggerFactory loggerFactory, IReverseProxyConfig reverseProxyConfig, FixedServersDnsResolver fixedServersDnsResolver, IServiceProvider serviceProvider)
    {
        this.loggerFactory = loggerFactory;
        this.serviceProvider = serviceProvider;
        this.reverseProxyConfig = reverseProxyConfig;
        this.fixedServersDnsResolver = fixedServersDnsResolver;
        resolver6 = new(FixedServersDnsResolver.CreateOptionsIPv6());
        dnsResolver = new(loggerFactory);
        memoryCache = CreateMemoryCache();
    }

    MemoryCache CreateMemoryCache()
    {
        var memoryCacheOptions = serviceProvider.GetService<IOptions<MemoryCacheOptions>>()?.Value ?? Options.Create(new MemoryCacheOptions());
        var memoryCache = new MemoryCache(memoryCacheOptions, loggerFactory);
        return memoryCache;
    }

    /// <inheritdoc/>
    public void ClearCache()
    {
        lock (lockClearCache)
        {
            var memoryCacheOld = this.memoryCache;
            var memoryCache = CreateMemoryCache();
            try
            {
                this.memoryCache = memoryCache;
            }
            finally
            {
                memoryCacheOld.Dispose();
            }
        }
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        resolver6?.Dispose();
        dnsResolver?.Dispose();
        resolver6 = null;
        dnsResolver = null;
        disposedValue = true;

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="IAsyncDisposable.DisposeAsync"/>
    public async ValueTask DisposeAsync()
    {
        if (resolver6 != null)
        {
            await resolver6.DisposeAsync().ConfigureAwait(false);
        }
        if (dnsResolver != null)
        {
            await dnsResolver.DisposeAsync().ConfigureAwait(false);
        }
        resolver6 = null;
        dnsResolver = null;
        disposedValue = true;

        GC.SuppressFinalize(this);
    }

    bool SupportsIPv6(DnsResult<AddressRecord> result)
    {
        if (result.ResponseCode == DnsResponseCode.NoError && result.Records.Count > 0)
        {
            var anyV6 = result.Records.Any(x => x.Address.AddressFamily == AddressFamily.InterNetworkV6);
            if (anyV6)
            {
                return true;
            }
        }
        return false;
    }

    bool? cacheSupportsIPv6;

    /// <inheritdoc/>
    public bool SupportsIPv6(bool fromCache = true)
    {
        if (Socket.OSSupportsIPv6)
        {
            if (fromCache && cacheSupportsIPv6.HasValue)
            {
                return cacheSupportsIPv6.Value;
            }

            ObjectDisposedException.ThrowIf(disposedValue || resolver6 == null, this);
            var result = resolver6.ResolveAddresses(IPV6_TESTDOMAIN);
            var supportsIPv6 = SupportsIPv6(result);
            cacheSupportsIPv6 = supportsIPv6;
            return supportsIPv6;
        }
        return false;
    }

    /// <inheritdoc/>
    public async ValueTask<bool> SupportsIPv6Async(bool fromCache = true, CancellationToken cancellationToken = default)
    {
        if (Socket.OSSupportsIPv6)
        {
            if (fromCache && cacheSupportsIPv6.HasValue)
            {
                return cacheSupportsIPv6.Value;
            }

            ObjectDisposedException.ThrowIf(disposedValue || resolver6 == null, this);
            var result = await resolver6.ResolveAddressesAsync(IPV6_TESTDOMAIN, cancellationToken);
            var supportsIPv6 = SupportsIPv6(result);
            cacheSupportsIPv6 = supportsIPv6;
            return supportsIPv6;
        }
        return false;
    }

    /// <summary>
    /// 获取用于缓存的字符串键
    /// </summary>
    static object GetCacheKey(string hostName, AddressFamily addressFamily) => new CacheKey(hostName, addressFamily);

    sealed record CacheKey(string HostName, AddressFamily AddressFamily);

    /// <inheritdoc/>
    public async Task<DnsResultWrapper<IPAddress>> ResolveAddressesAsync(string hostName, AddressFamily addressFamily = AddressFamily.Unspecified, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(hostName, addressFamily);
        if (memoryCache.TryGetValue<DnsResultWrapper<IPAddress>>(cacheKey, out var result))
        {
            // 从缓存中获取结果
            return result;
        }

        var r = await ResolveAddressesCoreAsync(hostName, addressFamily, cancellationToken).ConfigureAwait(false);
        var absoluteExpirationRelativeToNow = r.Result.GetAbsoluteExpirationRelativeToNow();

        result = r.Distinct();
        if (absoluteExpirationRelativeToNow > TimeSpan.Zero)
        {
            memoryCache.Set(cacheKey, result, absoluteExpirationRelativeToNow);
        }
        return result;
    }

    async Task<DnsResultWrapper<AddressRecord>> ResolveAddressesCoreAsync(string hostName, AddressFamily addressFamily = AddressFamily.Unspecified, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposedValue || dnsResolver == null, this);

        var supportsIPv6 = await SupportsIPv6Async(cancellationToken: cancellationToken);
        if (!supportsIPv6)
        {
            addressFamily = AddressFamily.InterNetwork; // 不支持 IPv6，则强制使用 IPv4
        }

        if (addressFamily == AddressFamily.Unspecified)
        {
            // if `async == true` then this runs both queries in parallel, otherwise it runs them sequentially (the synchronous path is expected to be rare and the async path is expected to be the common case).
            var aTask = DoResolve(dnsResolver, hostName, AddressFamily.InterNetwork, cancellationToken);
            var aaaaTask = DoResolve(dnsResolver, hostName, AddressFamily.InterNetworkV6, cancellationToken);

            await Task.WhenAll(aTask, aaaaTask).ConfigureAwait(false);
            var aaaaRes = await aaaaTask.ConfigureAwait(false);
            //if (iPv6Preferred)
            //{
            //    if (aaaaRes.Result.ResponseCode == DnsResponseCode.NoError && aaaaRes.Result.Records.Count > 0)
            //    {
            //        return aaaaRes; // IPv6 优先返回
            //    }
            //}
            var aRes = await aTask.ConfigureAwait(false);
            return aaaaRes.MergeAddressResults(aRes); // IPv6 优先，若没有则返回 IPv4
        }

        return await DoResolve(dnsResolver, hostName, addressFamily, cancellationToken).ConfigureAwait(false);
    }

    async Task<DnsResultWrapper<AddressRecord>> DoResolve(DnsComposeResolver dnsResolver, string hostName, AddressFamily addressFamily, CancellationToken cancellationToken = default)
    {
        if (reverseProxyConfig.ProxyMode is ReverseProxyMode.WinDivert or ReverseProxyMode.Hosts)
        {
            // Hosts 加速下不能使用系统默认 DNS 解析代理，会解析到 Hosts 或 DNS 拦截器上导致无限循环
            dnsResolver.UseSystemDefaultDns = false;
        }
        else
        {
            dnsResolver.UseSystemDefaultDns = true;
        }

        if (!dnsResolver.HasDnsResolver && !dnsResolver.HasDohResolver)
        {
            // 没有配置 DNS 服务器和 DoH 服务器时
            if (dnsResolver.UseSystemDefaultDns)
            {
                // 使用系统默认 DNS 解析
                return await Dns2.ResolveAddressesAsync(hostName, addressFamily, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // 使用预设的固定值 DNS 服务器解析
                return await fixedServersDnsResolver.ResolveAddressesAsync(hostName, addressFamily, cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            // 使用配置的 DNS 服务器或 DoH 服务器解析
            return await dnsResolver.ResolveAddressesAsync(hostName, addressFamily, cancellationToken).ConfigureAwait(false);
        }
    }
}