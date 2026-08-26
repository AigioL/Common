using AigioL.Common.Net.NameResolution.Abstractions;
using AigioL.Common.Net.NameResolution.Models;
using System.Net;
using System.Net.Sockets;
using static AigioL.Common.Net.NameResolution.Services.LoggerMessages;

namespace AigioL.Common.Net.NameResolution.Services;

/// <summary>
/// 多个实现组合的 DNS 记录解析器
/// </summary>
public class DnsComposeResolver : IDnsResolver, IAsyncDisposable, IDisposable
{
    readonly ILogger logger;
    readonly ILoggerFactory loggerFactory;
    DnsResolverWrapper? dnsResolver;
    DohResolver? dohResolver;

    public DnsComposeResolver(ILoggerFactory loggerFactory)
    {
        this.loggerFactory = loggerFactory;
        logger = loggerFactory.CreateLogger<DnsComposeResolver>();
    }

    /// <inheritdoc cref="DohResolver.Method"/>
    public HttpMethod? Method
    {
        get;
        set
        {
            dohResolver?.Method = value;
            field = value;
        }
    }

    /// <inheritdoc cref="DohResolver.UseRfc8484"/>
    public bool UseRfc8484
    {
        get;
        set
        {
            dohResolver?.UseRfc8484 = value;
            field = value;
        }
    }

    /// <summary>
    /// 是否使用系统默认的 DNS 解析
    /// </summary>
    public virtual bool UseSystemDefaultDns { get; set; } = true;

    /// <summary>
    /// 是否仅使用 DoH 解析，此选项为 <see langword="true"/> 时，将不使用系统默认 DNS 解析
    /// </summary>
    public virtual bool UseDohOnly { get; set; }

    /// <summary>
    /// 配置使用的 DNS 服务器 IP 地址
    /// </summary>
    public IList<IPEndPoint>? DnsServers
    {
        get;
        set
        {
            try
            {
                if (dnsResolver != null)
                {
                    if (dnsResolver.EqualsServers(value))
                    {
                        return;
                    }
                }
                CreateDnsResolver(value);
            }
            finally
            {
                field = value;
            }
        }
    }

    /// <summary>
    /// 配置使用的 DoH 服务器 Url 地址，地址必须是 https，通常地址末尾为 /dns-query
    /// </summary>
    public IReadOnlyList<Uri>? DohServers
    {
        get;
        set
        {
            try
            {
                if (dohResolver != null && value != null)
                {
                    if (dohResolver.EqualsServers(value))
                    {
                        return;
                    }
                }
                CreateDohResolver(value);
            }
            finally
            {
                field = value;
            }
        }
    }

    void CreateDnsResolver(IList<IPEndPoint>? servers)
    {
        var l = loggerFactory.CreateLogger<DnsResolverWrapper>();
        dnsResolver?.Dispose();
        if (servers == null || servers.Count == 0)
        {
            dnsResolver = null;
        }
        else
        {
            dnsResolver = CreateDnsResolver(l, servers);
        }
    }

    /// <summary>
    /// 可由派生类重写的创建 <see cref="DnsResolver"/>
    /// </summary>
    protected virtual DnsResolverWrapper CreateDnsResolver(ILogger<DnsResolverWrapper> logger, IList<IPEndPoint>? servers)
    {
        var dnsResolver = new DnsResolverWrapper(logger, servers);
        return dnsResolver;
    }

    void CreateDohResolver(IReadOnlyList<Uri>? servers)
    {
        var l = loggerFactory.CreateLogger<DohResolver>();
        dohResolver?.Dispose();
        if (servers == null || servers.Count == 0)
        {
            dohResolver = null;
        }
        else
        {
            dohResolver = CreateDohResolver(l, servers);
        }
    }

    /// <summary>
    /// 可由派生类重写的创建 <see cref="DohResolver"/>
    /// </summary>
    protected virtual DohResolver CreateDohResolver(ILogger<DohResolver> logger, IReadOnlyList<Uri> servers)
    {
        var client = CreateHttpClient();
        var dohResolver = new DohResolver(logger, client, servers);
        return dohResolver;
    }

    /// <summary>
    /// 可由派生类重写的创建 DoH 所需的 <see cref="HttpMessageHandler"/>
    /// </summary>
    /// <returns></returns>
    protected virtual HttpMessageHandler CreateHttpHandler()
    {
        SocketsHttpHandler h = new()
        {
            UseCookies = false,
            UseProxy = false,
        };
        return h;
    }

    /// <summary>
    /// 可由派生类重写的创建 DoH 所需的 <see cref="HttpClient"/>
    /// </summary>
    protected virtual HttpClient CreateHttpClient()
    {
        var h = CreateHttpHandler();
        var client = new HttpClient(h)
        {
            DefaultRequestVersion = HttpVersion.Version20, // 最低使用 HTTP/2
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact, // 仅使用 H2 发送
            Timeout = TimeSpan.FromSeconds(4), // 应用内 DoH/DoT 客户端：通常会自行设更短超时，常见 2–10 秒
        };
        return client;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // 释放托管状态(托管对象)
            dnsResolver?.Dispose();
            dohResolver?.Dispose();

            // 将大型字段设置为 null
            dnsResolver = null;
            dohResolver = null;
        }
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (dnsResolver is not null)
        {
            await dnsResolver.DisposeAsync().ConfigureAwait(false);
        }
        if (dohResolver is not null)
        {
            await dohResolver.DisposeAsync().ConfigureAwait(false);
        }

        dnsResolver = null;
        dohResolver = null;
    }

    /// <inheritdoc cref="IAsyncDisposable.DisposeAsync"/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async Task<DnsResultWrapper<AddressRecord>> ResolveAddressesAsync(string hostName, AddressFamily addressFamily, CancellationToken cancellationToken = default)
    {
        try
        {
            if (UseDohOnly)
            {
                if (dohResolver == null)
                {
                    // 没有配置任何 DoH 服务器
                    return DnsResponseCode.NotImplemented;
                }
                else
                {
                    var r = await dohResolver.ResolveAddressesAsync(hostName, addressFamily, cancellationToken);
                    return r;
                }
            }
            else
            {
                CancellationTokenSource cts = cancellationToken == default ? new() : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                List<Task<DnsResultWrapper<AddressRecord>>> ts = new(3);
                if (dohResolver != null)
                {
                    ts.Add(dohResolver.ResolveAddressesAsync(hostName, addressFamily, cancellationToken));
                }
                if (dnsResolver != null)
                {
                    ts.Add(dnsResolver.ResolveAddressesAsync(hostName, addressFamily, cancellationToken));
                }
                if (UseSystemDefaultDns || ts.Count == 0)
                {
                    ts.Add(Dns2.ResolveAddressesAsync(hostName, addressFamily, cancellationToken));
                }
                var r = await ts.ParallelWhenAnyAsync(DnsResultExtensions.HasValue, cts);
                return r;
            }
        }
        catch (OperationCanceledException)
        {
            return DnsResponseCode.ServerFailure;
        }
        catch (Exception ex)
        {
            LogErrorDnsComposeResolveAddresses(logger, ex, hostName, addressFamily);
            return DnsResponseCode.ServerFailure;
        }
    }
}

internal static partial class LoggerMessages
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "DNS Compose 解析主机名失败，hostName: {hostName}, addressFamily: {addressFamily}")]
    internal static partial void LogErrorDnsComposeResolveAddresses(ILogger logger, Exception? exception, string? hostName, AddressFamily addressFamily);
}