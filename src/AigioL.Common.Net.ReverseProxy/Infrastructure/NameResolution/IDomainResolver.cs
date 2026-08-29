using AigioL.Common.Net.NameResolution.Models;
using AigioL.Common.Net.NameResolution.Services;
using System.Net;
using System.Net.Sockets;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.NameResolution;

/// <summary>
/// 用于反向代理的域名解析器接口
/// </summary>
interface IDomainResolver : IAsyncDisposable, IDisposable
{
    DnsComposeResolver DnsCompose { get; }

    /// <summary>
    /// 异步解析指定主机名的指定地址族
    /// </summary>
    /// <param name="iPv6Preferred">当传递 <see cref="AddressFamily.Unspecified"/> 且支持 IPv6 时，是否优先仅返回 IPv6 地址</param>
    /// <param name="hostName">要解析的主机名</param>
    /// <param name="addressFamily">要查询的地址族。使用 <see cref="AddressFamily.InterNetwork"/> 表示 A 记录，<see cref="AddressFamily.InterNetworkV6"/> 表示 AAAA 记录，<see cref="AddressFamily.Unspecified"/> 表示两者兼有</param>
    /// <param name="cancellationToken">用于取消异步操作的令牌</param>
    /// <returns>一个任务，完成后会生成一个包含地址记录的 <see cref="DnsResult{T}"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="hostName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="hostName"/> is empty.</exception>
    /// <exception cref="ObjectDisposedException">The resolver has been disposed.</exception>
    Task<DnsResultWrapper<IPAddress>> ResolveAddressesAsync(string hostName, AddressFamily addressFamily = AddressFamily.Unspecified, CancellationToken cancellationToken = default);

    /// <summary>
    /// 是否支持 IPv6
    /// </summary>
    bool SupportsIPv6(bool fromCache = true);

    /// <summary>
    /// 是否支持 IPv6
    /// </summary>
    ValueTask<bool> SupportsIPv6Async(bool fromCache = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// 清理 DNS 解析缓存
    /// </summary>
    void ClearCache();
}
