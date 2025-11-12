using System.Net.Security;
using System.Net.Sockets;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Net.Http;

/// <summary>
/// 用于 Steam 社区的反向代理 HttpMessageHandler
/// </summary>
public sealed class SteamCommunityReverseProxyHttpClientHandler : DelegatingHandler
{
    const string DefaultReverseProxyHostNameOrAddress = "steamcommunity.rmbgame.net";

    readonly string reverseProxyHostNameOrAddress;
    readonly TimeSpan connectTimeout = TimeSpan.FromSeconds(10d);
    readonly TimeSpan responseDrainTimeout = TimeSpan.FromSeconds(10d);
    readonly TimeSpan pooledConnectionLifetime = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initializes a new instance of the <see cref="SteamCommunityReverseProxyHttpClientHandler"/> class.
    /// </summary>
    public SteamCommunityReverseProxyHttpClientHandler(CookieContainer? cookieContainer = null, string? reverseProxyHostNameOrAddress = null)
    {
        var handler = new SocketsHttpHandler()
        {
            Proxy = HttpNoProxy.Instance,
            UseProxy = false,
            UseCookies = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            ConnectCallback = ConnectCallback,
            ResponseDrainTimeout = responseDrainTimeout,
            PooledConnectionLifetime = pooledConnectionLifetime,
        };
        if (cookieContainer != null)
        {
            handler.UseCookies = true;
            handler.CookieContainer = cookieContainer;
        }
        InnerHandler = handler;
        this.reverseProxyHostNameOrAddress = string.IsNullOrWhiteSpace(reverseProxyHostNameOrAddress) ?
            DefaultReverseProxyHostNameOrAddress :
            reverseProxyHostNameOrAddress;
    }

    /// <summary>
    /// 发送请求
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri ?? throw new ApplicationException("The requested URI must be specified.");

        // 设置请求头 host，修改协议为 http
        request.Headers.Host = uri.Host;
        request.RequestUri = new UriBuilder(uri) { Scheme = Uri.UriSchemeHttp }.Uri;

        return await base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// 连接回调
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    async ValueTask<Stream> ConnectCallback(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        var innerExceptions = new List<Exception>();
        var ipEndPoints = (await Dns.GetHostAddressesAsync(reverseProxyHostNameOrAddress, cancellationToken)).Select(s => new IPEndPoint(s, context.DnsEndPoint.Port));

        foreach (var ipEndPoint in ipEndPoints)
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
                    $"HTTP connection to {ipEndPoint.Address} timed out."));
            }
            catch (Exception ex)
            {
                innerExceptions.Add(ex);
            }
        }

        throw new AggregateException("Could not find any IP that can be successfully connected.", innerExceptions);
    }

    /// <summary>
    /// 建立连接
    /// </summary>
    static async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext context, IPEndPoint ipEndPoint, CancellationToken cancellationToken)
    {
        var socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(ipEndPoint, cancellationToken);
        var stream = new NetworkStream(socket, ownsSocket: true);

        if (context.DnsEndPoint.Port != 443)
        {
            return stream;
        }

        var sslStream = new SslStream(stream, leaveInnerStreamOpen: false);
        await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
        {
            TargetHost = string.Empty,
            RemoteCertificateValidationCallback = (_, _, _, _) => true,
        }, cancellationToken);

        return sslStream;
    }
}