namespace AigioL.Common.Net.ReverseProxy.Models;

/// <summary>
/// 外部代理类型
/// </summary>
public enum ExternalProxyType : byte
{
    /// <summary>
    /// Http 代理
    /// </summary>
    Http = 0,

    /// <summary>
    /// Sock4 代理
    /// </summary>
    Socks4 = 1,

    /// <summary>
    /// Sock5 代理
    /// </summary>
    Socks5 = 2,
}
