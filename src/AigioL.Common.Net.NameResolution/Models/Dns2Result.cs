using System.Net;
using System.Runtime.CompilerServices;

namespace AigioL.Common.Net.NameResolution.Models;

/// <summary>
/// 表示 DNS 解析操作的结果，包括响应代码、解析记录以及（对于否定响应）否定缓存生存时间（TTL）
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct Dns2Result<T>
{
    readonly IReadOnlyList<T>? _records;

    /// <summary>
    /// 获取服务器返回的 DNS 响应代码
    /// </summary>
    public DnsResponseCode ResponseCode { get; }

    /// <summary>
    /// 获取服务器返回的记录，若出现错误或无数据响应，列表将为空
    /// </summary>
    public IReadOnlyList<T> Records => _records ?? [];

    /// <summary>
    /// 获取否定响应（NXDOMAIN 或 NODATA）可缓存的持续时间
    /// </summary>
    /// <remarks>
    /// 根据 RFC 2308 第 5 节，该值来源于授权部分中的 SOA 最小 TTL，可用性为尽力而为且依赖于平台；可用性为尽力而为且依赖于平台；当不适用或不可用时，其值为 <see cref="TimeSpan.Zero"/>
    /// </remarks>
    public TimeSpan NegativeCacheTtl { get; }

    public Dns2Result(DnsResponseCode responseCode, IReadOnlyList<T>? records = null, TimeSpan negativeCacheTtl = default)
    {
        ResponseCode = responseCode;
        _records = records;
        NegativeCacheTtl = negativeCacheTtl;
    }

    public static implicit operator Dns2Result<T>(DnsResponseCode responseCode) => new(responseCode);

    public static implicit operator DnsResult<T>(Dns2Result<T> r) => Unsafe.As<Dns2Result<T>, DnsResult<T>>(ref r);

    public static implicit operator Dns2Result<T>(DnsResult<T> r) => Unsafe.As<DnsResult<T>, Dns2Result<T>>(ref r);
}