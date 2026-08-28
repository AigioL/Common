using AigioL.Common.Net.ReverseProxy.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net;
using System.Runtime.CompilerServices;
using Yarp.ReverseProxy.Forwarder;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Http;

/// <summary>
/// HTTP 反向代理中间件
/// </summary>
sealed partial class HttpReverseProxyMiddleware
{
    readonly IHttpForwarder forwarder;
    readonly IReverseProxyHttpMessageInvokerFactory factory;
    readonly IReverseProxyConfig reverseProxyConfig;
    readonly ILogger logger;

#pragma warning disable IDE0290 // 使用主构造函数
    public HttpReverseProxyMiddleware(
#pragma warning restore IDE0290 // 使用主构造函数
        IHttpForwarder forwarder,
        IReverseProxyHttpMessageInvokerFactory factory,
        IReverseProxyConfig reverseProxyConfig,
        ILogger<HttpReverseProxyMiddleware> logger)
    {
        this.forwarder = forwarder;
        this.factory = factory;
        this.reverseProxyConfig = reverseProxyConfig;
        this.logger = logger;
    }

    static ArgumentOutOfRangeException GetUnknownHttpVersionException(string? actualValue, [CallerArgumentExpression(nameof(actualValue))] string? paramName = null) => new(
$"""
Version doesn't map to a known HTTP protocol. (Parameter '{paramName}')
Actual value was {actualValue}.
""");

    static Version GetHttpVersion(string requestProtocol)
    {
        // 参考 https://github.com/dotnet/aspnetcore/blob/v11.0.0-preview.7.26381.103/src/Http/Http.Abstractions/src/HttpProtocol.cs
        if (!string.IsNullOrWhiteSpace(requestProtocol) && requestProtocol.Length >= 6)
        {
            if (char.ToUpperInvariant(requestProtocol[0]) == 'H' &&
                char.ToUpperInvariant(requestProtocol[1]) == 'T' &&
                char.ToUpperInvariant(requestProtocol[2]) == 'T' &&
                char.ToUpperInvariant(requestProtocol[3]) == 'P' &&
                char.ToUpperInvariant(requestProtocol[4]) == '/')
            {
                if (requestProtocol.Length == 6)
                {
                    switch (requestProtocol[5])
                    {
                        case '1':
                            return HttpVersion.Version10;
                        case '2':
                            return HttpVersion.Version20;
                        case '3':
                            return HttpVersion.Version30;
                    }
                }

                if (requestProtocol.Length == 8 && requestProtocol[6] == '.')
                {
                    switch (requestProtocol[5])
                    {
                        case '1':
                            {
                                switch (requestProtocol[7])
                                {
                                    case '0':
                                        return HttpVersion.Version10;
                                    case '1':
                                        return HttpVersion.Version11;
                                }
                            }
                            break;
                        case '2':
                            {
                                switch (requestProtocol[7])
                                {
                                    case '0':
                                        return HttpVersion.Version20;
                                }
                            }
                            break;
                        case '3':
                            {
                                switch (requestProtocol[7])
                                {
                                    case '0':
                                        return HttpVersion.Version30;
                                }
                            }
                            break;
                    }
                }

                var requestProtocolVersion = requestProtocol.AsSpan(5);
                if (requestProtocolVersion.Contains('.'))
                {
                    if (Version.TryParse(requestProtocolVersion, out var v))
                    {
                        return v;
                    }
                }
                else
                {
                    if (int.TryParse(requestProtocolVersion, out var i))
                    {
                        return new Version(i, 0);
                    }
                }
            }
        }
        throw GetUnknownHttpVersionException(requestProtocol);
    }

    /// <summary>
    /// 处理请求
    /// </summary>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var displayUrl = context.Request.GetDisplayUrl();
    }
}
