namespace AigioL.Common.Net.NameResolution.Constants;

/// <summary>
/// DNS 常量
/// </summary>
public static partial class DnsConstants
{
    /// <summary>
    /// DNS 协议默认端口号
    /// </summary>
    public const int DefaultDnsPort = 53;

    /// <summary>
    /// 腾讯云公共 DNS IPv4 地址
    /// <para>https://www.dnspod.cn/products/publicdns</para>
    /// </summary>
    public const string DnspodIPv4 = "119.29.29.29";

    /// <summary>
    /// 腾讯云公共 DNS IPv6 地址
    /// <para>https://www.dnspod.cn/products/publicdns</para>
    /// </summary>
    public const string DnspodIPv6 = "2402:4e00::";

    /// <summary>
    /// 百度公共 DNS IPv4 地址
    /// <para>https://dudns.baidu.com/support/localdns/Address/index.html</para>
    /// </summary>
    public const string BaiduIPv4 = "180.76.76.76";

    /// <summary>
    /// 百度公共 DNS IPv6 地址
    /// <para>https://dudns.baidu.com/support/localdns/Address/index.html</para>
    /// </summary>
    public const string BaiduIPv6 = "2400:da00::6666";

    /// <summary>
    /// 114 公共 DNS IPv4 地址（主）
    /// <para>https://www.114dns.com/index.html</para>
    /// </summary>
    public const string NKG114IPv4Primary = "114.114.114.114";

    /// <summary>
    /// 114 公共 DNS IPv4 地址（备）
    /// <para>https://www.114dns.com/index.html</para>
    /// </summary>
    public const string NKG114IPv4Secondary = "114.114.115.115";

    /// <summary>
    /// 360 公共 DNS IPv4 地址（主）
    /// <para>https://sdns.360.net/dnsPublic.html#course</para>
    /// </summary>
    public const string Qihoo360IPv4Primary = "101.226.4.6";

    /// <summary>
    /// 360 公共 DNS IPv4 地址（备）
    /// <para>https://sdns.360.net/dnsPublic.html#course</para>
    /// </summary>
    public const string Qihoo360IPv4Secondary = "218.30.118.6";
}
