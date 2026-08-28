using Microsoft.AspNetCore.Http;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Http;

/// <summary>
/// HTTP 代理功能
/// </summary>
interface IHttpProxyFeature
{
    /// <summary>
    /// 代理的 HOST
    /// </summary>
    HostString ProxyHost { get; }

    /// <summary>
    /// 代理协议
    /// </summary>
    ProxyProtocol ProxyProtocol { get; }
}
