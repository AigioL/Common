using AigioL.Common.Net.NameResolution.Abstractions;
using AigioL.Common.Net.NameResolution.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static AigioL.Common.Net.NameResolution.Services.LoggerMessages;

namespace AigioL.Common.Net.NameResolution.Services;

/// <summary>
/// DNS over HTTPS (DoH) 记录解析器，可选地使用调用方指定的 DNS 服务器集
/// </summary>
public sealed class DohResolver : IDnsResolver, IAsyncDisposable, IDisposable
{
    HttpClient? client;
    IReadOnlyList<Uri>? servers;
    readonly ILogger logger;

    public DohResolver(ILogger<DohResolver> logger, HttpClient client, IReadOnlyList<Uri> servers)
    {
        this.logger = logger;
        this.servers = servers;
        this.client = client;
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        client?.Dispose();
        client = null;
        servers = null;

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="IAsyncDisposable.DisposeAsync"/>
    public async ValueTask DisposeAsync()
    {
        if (client != null)
        {
            if (client is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                client.Dispose();
            }
        }
        client = null;
        servers = null;

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 比较当前实例的 DNS 服务器集与指定的 DNS 服务器集是否相等
    /// </summary>
    /// <param name="servers">要比较的 DNS 服务器集</param>
    /// <returns>如果两个 DNS 服务器集相等，则返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    public bool EqualsServers(IReadOnlyList<Uri> servers)
    {
        if (this.servers == null)
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
            if (l == null)
            {
                return false;
            }
            var r = servers[i];
            if (!l.Equals(r))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 比较当前实例的 DNS 服务器集与指定的 DNS 服务器集是否相等
    /// </summary>
    /// <param name="servers">要比较的 DNS 服务器集</param>
    /// <returns>如果两个 DNS 服务器集相等，则返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    public bool EqualsServers(IReadOnlyList<string> servers)
    {
        if (this.servers == null)
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
            if (l == null)
            {
                return false;
            }
            var r = servers[i];
            if (!l.Equals(r))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 比较当前实例的 DNS 服务器集与指定的 <see cref="DohResolver"/> 实例的 DNS 服务器集是否相等
    /// </summary>
    /// <param name="dnsResolver">要比较的 <see cref="DohResolver"/> 实例</param>
    /// <returns>如果两个 DNS 服务器集相等，则返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    public bool EqualsServers(DohResolver dnsResolver)
    {
        var servers = dnsResolver.servers;
        if (servers == null)
        {
            return false;
        }
        return EqualsServers(servers);
    }

    /// <summary>
    /// 获取或设置用于发送 DoH 请求的 HTTP 方法（仅在使用 RFC 8484 规范有效），默认值为 <see cref="HttpMethod.Post"/>
    /// </summary>
    public HttpMethod? Method { get; set; }

    /// <summary>
    /// 是否使用 RFC 8484 规范进行 DoH 请求
    /// </summary>
    public bool UseRfc8484 { get; set; }

    /// <inheritdoc/>
    public async Task<DnsResultWrapper<AddressRecord>> ResolveAddressesAsync(string hostName, AddressFamily addressFamily = AddressFamily.Unspecified, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(client == null, this);
        ObjectDisposedException.ThrowIf(servers == null, this);

        if (servers.Count == 0)
        {
            // 没有配置任何 DoH 服务器
            return DnsResponseCode.NotImplemented;
        }

        CancellationTokenSource cts = cancellationToken == default ? new() : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        // 并发查询一组 DoH 服务器
        var ts = servers.Select(server => ResolveAddressesCoreAsync(client, server, hostName, addressFamily, cts.Token)).ToList();
        var r = await ts.ParallelWhenAnyAsync(DnsResultExtensions.HasValue, cts);
        return r;

        // TODO: 使用 System.Runtime.Caching.MemoryCache 将 DNS 结果进行缓存！
    }

    async Task<DnsResultWrapper<AddressRecord>> ResolveAddressesCoreAsync(HttpClient client, Uri server, string hostName, AddressFamily addressFamily = AddressFamily.Unspecified, CancellationToken cancellationToken = default)
    {
        if (addressFamily == AddressFamily.Unspecified)
        {
            // if `async == true` then this runs both queries in parallel, otherwise it runs them sequentially (the synchronous path is expected to be rare and the async path is expected to be the common case).
            var aTask = DoResolve(client, server, hostName, AddressFamily.InterNetwork, cancellationToken);
            var aaaaTask = DoResolve(client, server, hostName, AddressFamily.InterNetworkV6, cancellationToken);

            await Task.WhenAll(aTask, aaaaTask).ConfigureAwait(false);
            var aRes = await aTask.ConfigureAwait(false);
            var aaaaRes = await aaaaTask.ConfigureAwait(false);
            return aRes.MergeAddressResults(aaaaRes);
        }

        return await DoResolve(client, server, hostName, addressFamily, cancellationToken).ConfigureAwait(false);
    }

    async Task<DnsResultWrapper<AddressRecord>> DoResolve(HttpClient client, Uri server, string hostName, AddressFamily addressFamily, CancellationToken cancellationToken = default)
    {
        var timestamp = Stopwatch.GetTimestamp();
        try
        {
            if (UseRfc8484)
            {
                using var req = Rfc8484.GetRequest(server, Method, hostName);
                SetRequest(client, req);
                using var rsp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                throw new NotImplementedException("TODO: 待实现使用 RFC 8484 规范进行 DoH 请求");
                // TODO: 待实现使用 RFC 8484 规范进行 DoH 请求
            }
            else
            {
                var type = DnsResolverPal.AddressFamilyToQueryType(addressFamily);
                using var req = DohJsonApi.GetRequest(server, hostName, addressFamily);
                SetRequest(client, req);
                using var rsp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (!rsp.IsSuccessStatusCode)
                {
                    return DnsResponseCode.ServerFailure;
                }
                var m = await rsp.Content.ReadFromJsonAsync(DohJsonSerializerContext.Default.DohJsonResponseModel, cancellationToken);
                if (m == null)
                {
                    return DnsResponseCode.ServerFailure;
                }
                IReadOnlyList<AddressRecord>? records = null;
                if (m.Answer != null && m.Answer.Count > 0)
                {
                    records = [.. (from it in m.Answer
                               where it.Type == type
                               let address = (IPAddress2.TryParse(it.Data, out var address1) ? address1 : null)
                               where address != null
                               select new AddressRecord2(address, TimeSpan.FromSeconds(it.TTL)).ToBcl())];
                }
                return new Dns2Result<AddressRecord>(m.Status, records, default)
                    .ToWrapper(DnsResultSourceType.DohServers, server.OriginalString, elapsedTime: Stopwatch.GetElapsedTime(timestamp));
            }
        }
        catch (OperationCanceledException)
        {
            return DnsResponseCode.ServerFailure;
        }
        catch (Exception ex)
        {
            LogErrorDohResolveAddresses(logger, ex, server, hostName, addressFamily);
            return DnsResponseCode.ServerFailure;
        }
    }

    static void SetRequest(HttpClient c, HttpRequestMessage r)
    {
        r.Version = c.DefaultRequestVersion;
        r.VersionPolicy = c.DefaultVersionPolicy;
    }

    internal static class Rfc8484
    {
        /// <summary>
        /// https://github.com/dotnet/runtime/blob/v11.0.0-preview.7.26381.103/src/libraries/System.Text.Json/Common/JsonConstants.cs#L12
        /// </summary>
        const int StackallocByteThreshold = 256;
        const int StackallocCharThreshold = StackallocByteThreshold / 2;

        ref struct CharReadOnlySpan2
        {
            public ReadOnlySpan<char> Span1;
            public ReadOnlySpan<char> Span2;
        }

        static string GetGetUrl(ReadOnlySpan<char> server, ReadOnlySpan<char> base64UrlChars)
        {
            const string linkSpan = "dns=";
            var len = server.Length + 1 + linkSpan.Length + base64UrlChars.Length;
            var r = string.Create(len, new CharReadOnlySpan2 { Span1 = server, Span2 = base64UrlChars }, (span, args) =>
            {
                var server = args.Span1;
                var base64UrlChars = args.Span2;

                server.CopyTo(span);
                span[server.Length] = server.Contains('?') ? '&' : '?';

                var span2 = span[(server.Length + 1)..];
                linkSpan.CopyTo(span2);
                span2 = span2[linkSpan.Length..];
                base64UrlChars.CopyTo(span2);
            });
            return r;
        }

        internal static HttpRequestMessage GetRequest(Uri server, HttpMethod? method, string hostName)
        {
            bool isPost;

            if (method != HttpMethod.Get)
            {
                method = HttpMethod.Post;
                isPost = true;
            }
            else
            {
                isPost = method == HttpMethod.Post;
            }

            Uri requestUrl;
            HttpContent? content = null;
            if (isPost)
            {
                requestUrl = server;
            }
            else
            {
                int expectedByteCount = Encoding.UTF8.GetMaxByteCount(hostName.Length);
                byte[]? bytes = null;
                char[]? chars = null;
                Span<byte> utf8Bytes = expectedByteCount <= StackallocByteThreshold ?
                    stackalloc byte[StackallocByteThreshold] :
                    (bytes = ArrayPool<byte>.Shared.Rent(expectedByteCount));
                try
                {
                    var actualByteCount = Encoding.UTF8.GetBytes(hostName, utf8Bytes);
                    utf8Bytes = utf8Bytes[..actualByteCount];

                    int expectedCharCount = Base64Url.GetEncodedLength(utf8Bytes.Length);
                    Span<char> base64UrlChars = expectedCharCount <= StackallocCharThreshold ?
                        stackalloc char[StackallocCharThreshold] :
                        (chars = ArrayPool<char>.Shared.Rent(expectedCharCount));

                    Base64Url.TryEncodeToChars(utf8Bytes, base64UrlChars, out var charsWritten);
                    base64UrlChars = base64UrlChars[..charsWritten];
                    requestUrl = new Uri(GetGetUrl(server.OriginalString, base64UrlChars), UriKind.Absolute);
                }
                finally
                {
                    if (bytes is not null)
                    {
                        ArrayPool<byte>.Shared.Return(bytes);
                    }
                    if (chars is not null)
                    {
                        ArrayPool<char>.Shared.Return(chars);
                    }
                }
            }

            HttpRequestMessage request = new(method, requestUrl);
            if (content != null)
            {
                request.Content = content;
            }
            request.Headers.Accept.ParseAdd("application/dns-message");
            return request;
        }
    }

    /// <summary>
    /// 用于 DNS over HTTPS (DoH) 的 JSON API
    /// <para>https://www.alibabacloud.com/help/zh/dns/httpdns-doh-json-api</para>
    /// <para>https://developers.google.cn/speed/public-dns/docs/doh/json?hl=zh-cn</para>
    /// </summary>
    internal static class DohJsonApi
    {
        internal static HttpRequestMessage GetRequest(Uri server, string hostName, AddressFamily addressFamily)
        {
            Dictionary<string, string?> queryString = new()
        {
            { "name", hostName },
            { "type", DnsResolverPal.AddressFamilyToQueryStringValue(addressFamily) },
        };
            var requestUrl = QueryHelpers.AddQueryString(server.OriginalString, queryString);
            HttpRequestMessage request = new(HttpMethod.Get, requestUrl);
            request.Headers.Accept.ParseAdd("application/dns-json");
            return request;
        }
    }

}

file static class DnsResolverPal
{
    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v11.0.0-preview.7.26381.103/src/libraries/System.Net.NameResolution/src/Resources/Strings.resx#L90
    /// </summary>
    const string net_dns_unsupported_address_family = "Only the InterNetwork and InterNetworkV6 address families are supported.";

    internal static string AddressFamilyToQueryStringValue(AddressFamily addressFamily) =>
          addressFamily switch
          {
              AddressFamily.InterNetwork => STR_DNS_TYPE_A,
              AddressFamily.InterNetworkV6 => STR_DNS_TYPE_AAAA,
              _ => throw new ArgumentException(net_dns_unsupported_address_family, nameof(addressFamily)),
          };

    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v11.0.0-preview.7.26381.103/src/libraries/System.Net.NameResolution/src/System/Net/DnsResolverPal.Windows.cs#L88
    /// </summary>
    internal static ushort AddressFamilyToQueryType(AddressFamily addressFamily) =>
        addressFamily switch
        {
            AddressFamily.InterNetwork => DNS_TYPE_A,
            AddressFamily.InterNetworkV6 => DNS_TYPE_AAAA,
            _ => throw new ArgumentException(net_dns_unsupported_address_family, nameof(addressFamily)),
        };

    #region Dnsapi

    // https://github.com/dotnet/runtime/blob/v11.0.0-preview.7.26381.103/src/libraries/Common/src/Interop/Windows/Dnsapi/Interop.DnsApi.cs#L9
    // https://www.alibabacloud.com/help/zh/dns/httpdns-doh-json-api#p-niu-itd-5yo

    internal const ushort DNS_TYPE_A = 0x0001;
    internal const ushort DNS_TYPE_AAAA = 0x001c;

    internal const string STR_DNS_TYPE_A = "A";
    internal const string STR_DNS_TYPE_AAAA = "AAAA";

    #endregion
}

internal static partial class LoggerMessages
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "DoH 解析主机名失败，server: {server}, hostName: {hostName}, addressFamily: {addressFamily}")]
    internal static partial void LogErrorDohResolveAddresses(ILogger logger, Exception? exception, Uri? server, string? hostName, AddressFamily addressFamily);
}