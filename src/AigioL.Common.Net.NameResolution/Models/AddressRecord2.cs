using System.Net;
using System.Runtime.CompilerServices;

namespace AigioL.Common.Net.NameResolution.Models;

/// <summary>
/// 表示从 DNS 解析出的 A 或 AAAA 记录，包括其生存时间
/// </summary>
public readonly struct AddressRecord2
{
    /// <summary>
    /// 获取解析后的 IP 地址
    /// </summary>
    public IPAddress Address { get; }

    /// <summary>
    /// 获取记录的生存时间（TTL）
    /// </summary>
    public TimeSpan Ttl { get; }

    public AddressRecord2(IPAddress address, TimeSpan ttl)
    {
        Address = address;
        Ttl = ttl;
    }

    public static implicit operator AddressRecord(AddressRecord2 r) => Unsafe.As<AddressRecord2, AddressRecord>(ref r);

    public static implicit operator AddressRecord2(AddressRecord r) => Unsafe.As<AddressRecord, AddressRecord2>(ref r);

    public AddressRecord ToBcl()
    {
        AddressRecord thiz = this;
        return thiz;
    }
}