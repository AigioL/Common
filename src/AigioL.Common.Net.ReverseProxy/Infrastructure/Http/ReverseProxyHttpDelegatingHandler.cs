using AigioL.Common.Net.NameResolution.Abstractions;
using AigioL.Common.Net.NameResolution.Models;
using AigioL.Common.Net.ReverseProxy.Infrastructure.Configuration;
using AigioL.Common.Net.ReverseProxy.Models;
using System.Buffers;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Utf8StringInterpolation;
using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Http;

sealed class ReverseProxyHttpDelegatingHandler : DelegatingHandler
{
    const int DefaultConnectTimeoutFromSeconds = 10;

    readonly IDomainConfig domainConfig;
    readonly IDnsResolver dnsResolver;
    readonly IReverseProxyConfig reverseProxyConfig;
    readonly TimeSpan connectTimeout;

#pragma warning disable IDE0290 // 使用主构造函数
    public ReverseProxyHttpDelegatingHandler(
        IDomainConfig domainConfig,
        IDnsResolver dnsResolver,
        IReverseProxyConfig reverseProxyConfig,
        TimeSpan? connectTimeout = null)
#pragma warning restore IDE0290 // 使用主构造函数
    {
        this.domainConfig = domainConfig;
        this.dnsResolver = dnsResolver;
        this.reverseProxyConfig = reverseProxyConfig;
        this.connectTimeout = connectTimeout ?? TimeSpan.FromSeconds(DefaultConnectTimeoutFromSeconds);
    }

    async Task<HttpResponseMessage> SendAsync(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> @base, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 注意！此函数因重写 override Send，所以不能有真正的 await 调用异步

        ArgumentNullException.ThrowIfNull(request.RequestUri);

        // 请求上下文信息
        var isHttps = request.RequestUri.Scheme == Uri.UriSchemeHttps;
        request.Options.SetIsHttps(isHttps);
        var tlsSniValue = domainConfig.GetTlsSniPattern().WithDomain(request.RequestUri.Host).WithRandom();
        request.Options.SetTlsSniPattern(tlsSniValue);

        // 设置请求头 host
        request.Headers.Host = request.RequestUri.Host;

        HttpResponseMessage rsp;
        if (domainConfig.Timeout != null)
        {
            using var timeoutTokenSource = new CancellationTokenSource(domainConfig.Timeout.Value);
            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token);
            rsp = await @base(request, linkedTokenSource.Token);
        }
        else
        {
            rsp = await @base(request, cancellationToken);
        }
        return rsp;
    }

    Task<HttpResponseMessage> BaseSend(HttpRequestMessage req, CancellationToken token) => Task.FromResult(base.Send(req, token));

    protected sealed override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return SendAsync(BaseSend, request, cancellationToken).Result; // 这里内部调用链没有真正的 await，都是包装 Task.FromResult，所以不会有死锁问题
    }

    protected sealed override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return SendAsync(base.SendAsync, request, cancellationToken);
    }

    /// <summary>
    /// 创建转发代理的 <see cref="HttpMessageHandler"/>
    /// </summary>
    /// <returns></returns>
    SocketsHttpHandler CreateSocketsHttpHandler() => new()
    {
        Proxy = HttpNoProxy.Instance,
        UseProxy = false,
        PreAuthenticate = false,
        UseCookies = false,
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.None,
        ConnectCallback = ConnectCallback,
        EnableMultipleHttp2Connections = true,
        EnableMultipleHttp3Connections = true,
        RequestHeaderEncodingSelector = RequestHeaderEncodingSelector,
        ResponseHeaderEncodingSelector = ResponseHeaderEncodingSelector,
    };

    /// <summary>
    /// 解析要对请求标头值进行编码的 <see cref="Encoding"/>
    /// </summary>
    Encoding? RequestHeaderEncodingSelector(string headerName, HttpRequestMessage req)
    {
        return Encoding.UTF8;
    }

    /// <summary>
    /// 解析要对响应标头值进行编码的 <see cref="Encoding"/>
    /// </summary>
    Encoding? ResponseHeaderEncodingSelector(string headerName, HttpRequestMessage req)
    {
        return Encoding.UTF8;
    }

    /// <summary>
    /// 打开新连接的自定义回调
    /// </summary>
    async ValueTask<Stream> ConnectCallback(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        if (reverseProxyConfig.SecondLevelWebProxy != null && !HttpNoProxy.IsNoProxy(reverseProxyConfig.SecondLevelWebProxy))
        {
            return await ConnectThroughProxyAsync(context, cancellationToken);
        }

        var innerExceptions = new List<Exception>();
        var ipEndPoints = GetIPEndPointsAsync(context.DnsEndPoint, cancellationToken);

        await foreach (var ipEndPoint in ipEndPoints)
        {
            try
            {
                using var timeoutTokenSource = new CancellationTokenSource(connectTimeout);
                using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, cancellationToken);
                return await ConnectAsync(context, ipEndPoint, linkedTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                cancellationToken.ThrowIfCancellationRequested();
                innerExceptions.Add(new TimeoutException(
                    $"与 {ipEndPoint.IPEndPoint} 的 HTTP 连接已超时，来源：{ipEndPoint.SourceType}，追溯键：{ipEndPoint.TraceId}，耗时：{ipEndPoint.ElapsedTime}"));
            }
            catch (Exception ex)
            {
                innerExceptions.Add(ex);
            }
        }

        throw new AggregateException("未找到任何可以成功连接的 IP 地址", innerExceptions);
    }

    /// <summary>
    /// 打开新连接的自定义回调（使用二级代理时）
    /// </summary>
    async ValueTask<Stream> ConnectThroughProxyAsync(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context.InitialRequestMessage.RequestUri);
        var requestUri = context.InitialRequestMessage.RequestUri;
        var proxyUri = reverseProxyConfig.SecondLevelWebProxy.GetProxy(requestUri);
        ArgumentNullException.ThrowIfNull(proxyUri);

        var proxyEndPoint = new DnsEndPoint(proxyUri.Host, proxyUri.Port);
        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(proxyEndPoint, cancellationToken);

        var stream = new NetworkStream(socket, ownsSocket: true);

        // 确定代理类型
        var proxyType = DetermineProxyType(proxyUri);

        switch (proxyType)
        {
            case ExternalProxyType.Http:
                ConnectHttpProxy(stream, context, cancellationToken);
                break;
            case ExternalProxyType.Socks4:
                await ConnectSocks4Proxy(stream, context, cancellationToken);
                break;
            case ExternalProxyType.Socks5:
                ConnectSocks5Proxy(stream, context, cancellationToken);
                break;
        }

        if (string.Equals(Uri.UriSchemeHttps, context.InitialRequestMessage.RequestUri.Scheme, StringComparison.InvariantCultureIgnoreCase) ||
            context.InitialRequestMessage.RequestUri.Port == 443)
        {
            var tlsSniValue = context.InitialRequestMessage.Options.GetTlsSniPattern();
            var sslStream = new SslStream(stream, leaveInnerStreamOpen: false);
            await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
            {
                TargetHost = tlsSniValue.Value,
                RemoteCertificateValidationCallback = (_, _, _, _) => true,
            }, cancellationToken);

            return sslStream;
        }

        return stream;
    }

    /// <summary>
    /// 根据代理 Url 解析外部代理类型
    /// </summary>
    ExternalProxyType DetermineProxyType(Uri proxyUri)
    {
        var scheme = proxyUri.Scheme;
        if (string.Equals("socks4", scheme, StringComparison.InvariantCultureIgnoreCase))
        {
            return ExternalProxyType.Socks4;
        }
        else if (string.Equals("socks5", scheme, StringComparison.InvariantCultureIgnoreCase))
        {
            return ExternalProxyType.Socks5;
        }
        else
        {
            return ExternalProxyType.Http;
        }
    }

    void WriteConnectRequest(Stream stream, SocketsHttpConnectionContext context)
    {
        using var buffer = Utf8String.CreateWriter(out var builder);

        builder.AppendUtf8("CONNECT "u8);
        builder.Append(context.DnsEndPoint.Host);
        builder.AppendUtf8(":"u8);
        builder.AppendFormatted(context.DnsEndPoint.Port);
        builder.AppendUtf8("HTTP/1.1\r\n"u8);

        builder.AppendUtf8("Host: "u8);
        builder.Append(context.DnsEndPoint.Host);
        builder.AppendUtf8(":"u8);
        builder.AppendFormatted(context.DnsEndPoint.Port);
        builder.AppendUtf8("\r\n"u8);

        if (reverseProxyConfig.SecondLevelWebProxy.Credentials is NetworkCredential credentials)
        {
            // authHeader
            builder.AppendUtf8("Proxy-Authorization: Basic "u8);

            using var buffer2 = Utf8String.CreateWriter(out var builder2);
            builder2.Append(credentials.UserName);
            builder2.AppendUtf8(":"u8);
            builder2.Append(credentials.Password);
            builder2.Flush();

            builder.AppendBase64String(buffer2.WrittenSpan);

            builder.AppendUtf8("\r\n"u8);
        }

        builder.Flush();
        stream.Write(buffer.WrittenSpan);
    }

    void ConnectHttpProxy(NetworkStream stream, SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        WriteConnectRequest(stream, context);
        var u8Http11 = "HTTP/1.1 200"u8;
        {
            Span<byte> buffer = stackalloc byte[u8Http11.Length];
            var len = stream.Read(buffer);
            if (len != u8Http11.Length ||
                !buffer.SequenceEqual(u8Http11, Utf8StringComparerOrdinalIgnoreCase.Instance))
            {
                // 读取的值不是 HTTP/1.1 200
                throw new HttpIOException(HttpRequestError.ProxyTunnelError,
                    "代理连接失败，读取到的不是 HTTP/1.1 200");
            }
        }
    }

    async Task ConnectSocks4Proxy(NetworkStream stream, SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        var destinationAddress = await GetIPEndPointsAsync(context.DnsEndPoint, cancellationToken).FirstAsync(cancellationToken);
        Span<byte> portBytes = stackalloc byte[sizeof(ushort)];
        var port16 = unchecked((ushort)context.DnsEndPoint.Port);
        if (!BitConverter.TryWriteBytes(portBytes, port16))
        {
            portBytes = BitConverter.GetBytes(port16);
        }
        if (BitConverter.IsLittleEndian)
        {
            portBytes.Reverse();
        }

        stream.WriteByte(0x04);
        stream.WriteByte(0x01);
        stream.Write(portBytes);
        stream.Write(destinationAddress.IPEndPoint.Address.GetAddressBytes());
        if (reverseProxyConfig.SecondLevelWebProxy.Credentials is NetworkCredential credentials)
        {
            stream.WriteUtf8(credentials.UserName);
        }
        stream.WriteByte(0x00);
        {
            const int bufferSize = 2;
            Span<byte> buffer = stackalloc byte[bufferSize];
            var len = stream.Read(buffer);
            if (len != bufferSize || buffer[1] != 0x5A)
            {
                throw new HttpIOException(HttpRequestError.ProxyTunnelError,
                    $"SOCKS4 代理连接失败：{buffer[1]:X}");
            }
        }
    }

    void ConnectSocks5Proxy(NetworkStream stream, SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        // SOCKS5 握手
        byte authMethod;
        NetworkCredential? credentials = null;
        if (reverseProxyConfig.SecondLevelWebProxy.Credentials is NetworkCredential networkCredential)
        {
            credentials = networkCredential;
            authMethod = 0x02;
        }
        else
        {
            authMethod = 0x00;
        }

        stream.WriteByte(0x05);
        stream.WriteByte(0x01);
        stream.WriteByte(authMethod);

        var isAuth = false;
        {
            const int bufferSize = 2;
            Span<byte> buffer = stackalloc byte[bufferSize];
            var len = stream.Read(buffer);
            if (len != bufferSize || buffer.Length < 2 || buffer[1] != 0x5A)
            {
                throw new HttpIOException(HttpRequestError.ProxyTunnelError,
                    $"SOCKS5 握手失败：{buffer[1]:X}");
            }
            isAuth = buffer[1] == 0x02;
        }

        // 如果需要身份验证
        if (isAuth && credentials != null)
        {
            stream.WriteByte(0x01);
            stream.WriteUtf8WithLength(credentials.UserName, ThrowHelper.GetArgumentOutOfRangeException(credentials.UserName));
            stream.WriteUtf8WithLength(credentials.Password, ThrowHelper.GetArgumentOutOfRangeException(credentials.Password));
            {
                const int bufferSize = 2;
                Span<byte> buffer = stackalloc byte[bufferSize];
                var len = stream.Read(buffer);
                if (len != bufferSize || buffer.Length < 2 || buffer[1] != 0x00)
                {
                    throw new HttpIOException(HttpRequestError.ProxyTunnelError,
                        $"SOCKS5 认证失败：{buffer[1]:X}");
                }
            }
        }

        // 发送连接请求
        Span<byte> addressBytes = stackalloc byte[IPv6AddressBytes];
        byte addressType;
        if (IPAddress2.TryParse(context.DnsEndPoint.Host, out IPAddress? ipAddress))
        {
            if (ipAddress.TryWriteBytes(addressBytes, out var bytesWritten))
            {
                addressBytes = addressBytes[..bytesWritten];
            }
            addressType = (byte)(ipAddress.AddressFamily == AddressFamily.InterNetwork ? 1 : 4);
        }
        else
        {
            addressBytes = Encoding.UTF8.GetBytes(context.DnsEndPoint.Host);
            addressType = 3; // 域名
        }

        stream.WriteByte(0x05);
        stream.WriteByte(0x01);
        stream.WriteByte(0x00);
        stream.WriteByte(addressType);
        if (addressType == 3)
        {
            stream.WriteUtf8WithLength(context.DnsEndPoint.Host, ThrowHelper.GetArgumentOutOfRangeException(context.DnsEndPoint.Host));
        }
        else
        {
            stream.Write(addressBytes);
        }

        Span<byte> dnsPort = addressBytes.Length >= sizeof(ushort) ? addressBytes : stackalloc byte[sizeof(ushort)]; // 复用上一个 stackalloc 缓冲区
        var port16 = unchecked((ushort)context.DnsEndPoint.Port);
        if (!BitConverter.TryWriteBytes(dnsPort, port16))
        {
            dnsPort = BitConverter.GetBytes(port16);
        }
        dnsPort.Reverse();
        stream.Write(dnsPort);
        {
            const int bufferSize = 2;
            Span<byte> buffer = addressBytes.Length >= sizeof(ushort) ? addressBytes[..bufferSize] : stackalloc byte[bufferSize]; // 复用上一个 stackalloc 缓冲区
            var len = stream.Read(buffer);
            if (len != bufferSize || buffer.Length < 2 || buffer[1] != 0x00)
            {
                throw new HttpIOException(HttpRequestError.ProxyTunnelError,
                    $"SOCKS5 连接失败：{buffer[1]:X}");
            }
        }
    }

    /// <summary>
    /// 解析为 <see cref="IPEndPoint"/>
    /// </summary>
    async IAsyncEnumerable<IPEndPointWrapper> GetIPEndPointsAsync(DnsEndPoint dnsEndPoint, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (IPAddress2.TryParse(dnsEndPoint.Host, out var address))
        {
            yield return new IPEndPointWrapper(new(address, dnsEndPoint.Port))
            {
                TraceId = "D_IP", // 直连 IP 地址
            };
        }
        else
        {
            if (domainConfig.IPAddress != null)
            {
                yield return new IPEndPointWrapper(new IPEndPoint(domainConfig.IPAddress, dnsEndPoint.Port))
                {
                    TraceId = "DC_IP", // 来自域名配置的 IP 地址
                };
            }

            DnsResultWrapper<AddressRecord> result;
            if (domainConfig.ForwardDestination != null)
            {
                result = await dnsResolver.ResolveAddressesAsync(domainConfig.ForwardDestination, AddressFamily.Unspecified, cancellationToken);
                if (result.Result.Records.Count != 0)
                {
                    foreach (var it in result.Result.Records)
                    {
                        yield return new IPEndPointWrapper(it, result, dnsEndPoint.Port, "FD_IP_"); // 来自转发目标的 IP 地址
                    }
                }
            }

            result = await dnsResolver.ResolveAddressesAsync(dnsEndPoint.Host, AddressFamily.Unspecified, cancellationToken);
            if (result.Result.Records.Count != 0)
            {
                foreach (var it in result.Result.Records)
                {
                    yield return new IPEndPointWrapper(it, result, dnsEndPoint.Port); // 来自 DNS 解析的 IP 地址
                }
            }
        }
    }

    /// <summary>
    /// 建立连接
    /// </summary>
    async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext context, IPEndPointWrapper ipEndPoint, CancellationToken cancellationToken)
    {
        var socket = new Socket(ipEndPoint.IPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(ipEndPoint.IPEndPoint, cancellationToken);
        var stream = new NetworkStream(socket, ownsSocket: true);

        var isHttps = context.InitialRequestMessage.Options.GetIsHttps();
        if (!isHttps)
        {
            return stream;
        }
        var tlsSniValue = context.InitialRequestMessage.Options.GetTlsSniPattern().WithIPAddress(ipEndPoint.IPEndPoint.Address);
        var sslStream = new SslStream(stream, leaveInnerStreamOpen: false);
        await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
        {
            TargetHost = tlsSniValue.Value,
            RemoteCertificateValidationCallback = ValidateServerCertificate
        }, cancellationToken);

        return sslStream;

        // 验证证书有效性
        bool ValidateServerCertificate(object sender, X509Certificate? cert, X509Chain? chain, SslPolicyErrors errors)
        {
            if (errors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
            {
                if (domainConfig.TlsIgnoreNameMismatch)
                {
                    return true;
                }

                var domain = context.DnsEndPoint.Host;
                var dnsNames = GetDnsNames(cert);
                var r = dnsNames.Any(dns => IsMatchDomain(dns, domain));
                return r;
            }

            return errors == SslPolicyErrors.None;
        }
    }

    /// <summary>
    /// 从 X509 证书中读取 DNS 名称
    /// </summary>
    static IEnumerable<string> GetDnsNames(X509Certificate? x509Cert)
    {
        if (x509Cert is X509Certificate2 x509Cert2)
        {
            var extension = x509Cert2.Extensions.OfType<X509SubjectAlternativeNameExtension>().FirstOrDefault();
            if (extension != null)
            {
                return extension.EnumerateDnsNames();
            }
        }
        return [];
    }

    /// <summary>
    /// 用于域名的比较是否匹配，支持通配符
    /// </summary>
    static bool IsMatchDomain(string dnsName, string? domain)
    {
        if (domain == null)
        {
            return false;
        }
        if (dnsName == domain)
        {
            return true;
        }
        if (dnsName[0] == '*')
        {
            return domain.AsSpan().EndsWith(dnsName.AsSpan()[1..], StringComparison.InvariantCultureIgnoreCase);
        }
        return false;
    }
}

readonly struct IPEndPointWrapper
{
    public IPEndPointWrapper(IPEndPoint iPEndPoint)
    {
        IPEndPoint = iPEndPoint;
    }

    public IPEndPointWrapper(AddressRecord record, DnsResultWrapper<AddressRecord> result, int port, string? addTraceId = null)
    {
        IPEndPoint = new IPEndPoint(record.Address, port);
        TraceId = string.IsNullOrEmpty(addTraceId) ? result.TraceId : addTraceId + result.TraceId;
        ElapsedTime = result.ElapsedTime;
        SourceType = result.SourceType;
    }

    /// <summary>
    /// 结果的来源类型
    /// </summary>
    public DnsResultSourceType SourceType { get; init; }

    /// <summary>
    /// 用于跟踪的可选字段
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// DNS 返回成功结果耗时
    /// </summary>
    public TimeSpan? ElapsedTime { get; init; }

    public IPEndPoint IPEndPoint { get; init; }
}