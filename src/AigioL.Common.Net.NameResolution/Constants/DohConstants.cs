namespace AigioL.Common.Net.NameResolution.Constants;

/// <summary>
/// DNS-over-HTTPS (DoH) 常量
/// </summary>
public static partial class DohConstants
{
    /// <summary>
    /// 腾讯云公共 DNS-over-HTTPS (DoH)
    /// <para>https://www.dnspod.cn/products/publicdns</para>
    /// </summary>
    public const string Dnspod = "https://doh.pub/dns-query"; // https://docs.dnspod.cn/notices/mian-fei-ban-dot-dohbu-zai-gong-kai-ipjie-ru-de-gong-gao Dnspod 已不支持通过 IP 访问 DoH 服务

    /// <summary>
    /// 谷歌公共 DNS-over-HTTPS (DoH)
    /// <para>https://dns.google/dns-query</para>
    /// <para>https://developers.google.com/speed/public-dns/docs/doh?hl=zh-cn</para>
    /// </summary>
    public const string Google = "https://dns.google/dns-query";

    /// <summary>
    /// Cloudflare 公共 DNS-over-HTTPS (DoH)
    /// <para>https://developers.cloudflare.com/1.1.1.1/dns-over-https</para>
    /// </summary>
    public const string Cloudflare = "https://cloudflare-dns.com/dns-query";

    /// <summary>
    /// 360 公共 DNS-over-HTTPS (DoH)
    /// <para>https://sdns.360.net/dnsPublic.html#course</para>
    /// <para>https://doh.360.cn/dns-query</para>
    /// </summary>
    public const string Qihoo360 = "https://doh.360.cn/dns-query";
}
