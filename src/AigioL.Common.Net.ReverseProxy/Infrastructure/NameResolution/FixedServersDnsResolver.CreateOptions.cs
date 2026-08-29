using System.Net;
using static AigioL.Common.Net.NameResolution.Constants.DnsConstants;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.NameResolution;

partial class FixedServersDnsResolver
{
    /// <summary>
    /// 使用预设的一批 IPv4 DNS 服务器创建 <see cref="DnsResolverOptions"/> 实例
    /// </summary>
    internal static DnsResolverOptions CreateOptionsIPv4()
    {
        IPEndPoint[] servers =
        [
            new IPEndPoint(IPAddress.Parse(DnspodIPv4), DefaultDnsPort),
            new IPEndPoint(IPAddress.Parse(BaiduIPv4), DefaultDnsPort),
            new IPEndPoint(IPAddress.Parse(Qihoo360IPv4Primary), DefaultDnsPort),
        ];
        return new()
        {
            Servers = servers,
        };
    }

    /// <summary>
    /// 使用预设的一批 IPv6 DNS 服务器创建 <see cref="DnsResolverOptions"/> 实例
    /// </summary>
    internal static DnsResolverOptions CreateOptionsIPv6()
    {
        IPEndPoint[] servers =
        [
            new(IPAddress.Parse(DnspodIPv6), DefaultDnsPort),
            new(IPAddress.Parse(BaiduIPv6), DefaultDnsPort),
        ];
        return new()
        {
            Servers = servers,
        };
    }
}
