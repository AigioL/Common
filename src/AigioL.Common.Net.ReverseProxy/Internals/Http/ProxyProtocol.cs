namespace AigioL.Common.Net.ReverseProxy.Internals.Http;

/// <summary>
/// 代理协议
/// </summary>
enum ProxyProtocol : byte
{
    /// <summary>
    /// 无代理
    /// </summary>
    None = 0,

    /// <summary>
    /// Http 代理
    /// </summary>
    HttpProxy = 1,

    /// <summary>
    /// 隧道代理
    /// </summary>
    TunnelProxy = 2,
}