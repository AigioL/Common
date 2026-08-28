using System.Net;
using static AigioL.Common.Net.NameResolution.Constants.DnsConstants;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.NameResolution;

partial class FixedServersDnsResolver
{
    static DnsResolverOptions CreateOptions()
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
}
