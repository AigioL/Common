using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using System.Text;
using HttpMethod = global::Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpMethod;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Http;

/// <summary>
/// 代理请求处理
/// </summary>
sealed class HttpRequestHandler : IHttpRequestLineHandler, IHttpHeadersHandler, IHttpProxyFeature
{
    HttpMethod method;

    public HostString ProxyHost { get; private set; }

    public ProxyProtocol ProxyProtocol
    {
        get
        {
            if (ProxyHost.HasValue == false)
            {
                return ProxyProtocol.None;
            }
            else if (method == HttpMethod.Connect)
            {
                return ProxyProtocol.TunnelProxy;
            }
            return ProxyProtocol.HttpProxy;
        }
    }

    /// <inheritdoc/>
    void IHttpRequestLineHandler.OnStartLine(HttpVersionAndMethod versionAndMethod, TargetOffsetPathLength targetPath, Span<byte> startLine)
    {
        method = versionAndMethod.Method;
        var host = Encoding.UTF8.GetString(startLine.Slice(targetPath.Offset, targetPath.Length));
        if (versionAndMethod.Method == HttpMethod.Connect)
        {
            ProxyHost = HostString.FromUriComponent(host);
        }
        else if (Uri.TryCreate(host, UriKind.Absolute, out var uri))
        {
            ProxyHost = HostString.FromUriComponent(uri);
        }
    }

    /// <inheritdoc/>
    void IHttpHeadersHandler.OnHeader(ReadOnlySpan<byte> name, ReadOnlySpan<byte> value)
    {
    }

    /// <inheritdoc/>
    void IHttpHeadersHandler.OnHeadersComplete(bool endStream)
    {
    }

    /// <inheritdoc/>
    void IHttpHeadersHandler.OnStaticIndexedHeader(int index)
    {
    }

    /// <inheritdoc/>
    void IHttpHeadersHandler.OnStaticIndexedHeader(int index, ReadOnlySpan<byte> value)
    {
    }
}
