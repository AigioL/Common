using AigioL.Common.Net.NameResolution.Constants;
using AigioL.Common.Net.NameResolution.Models;
using AigioL.Common.Net.NameResolution.Services;
using System.Buffers.Text;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace AigioL.Common.UnitTest;

public sealed class NameResolutionTest
{
    /// <summary>
    /// 检查两个结构体是否可以通过 Unsafe.As 进行转换
    /// </summary>
    static void StructAsTest<TL, TR>(TL l = default)
        where TL : struct
        where TR : struct
    {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
        Assert.True(sizeof(TL) == sizeof(TR));
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针

        TR r = Unsafe.As<TL, TR>(ref l);
        l = Unsafe.As<TR, TL>(ref r);
    }

    [Fact]
    public void AddressRecordTest()
    {
        StructAsTest<AddressRecord, AddressRecord2>();
        StructAsTest<AddressRecord2, AddressRecord>(new(IPAddress.IPv6Loopback, TimeSpan.FromSeconds(30)));
    }

    [Fact]
    public void DnsResultTest()
    {
        StructAsTest<DnsResult<AddressRecord>, Dns2Result<AddressRecord>>();
        StructAsTest<DnsResult<AddressRecord>, Dns2Result<AddressRecord2>>();
        StructAsTest<Dns2Result<AddressRecord>, DnsResult<AddressRecord>>(new(DnsResponseCode.NoError, [], TimeSpan.FromSeconds(3)));
        StructAsTest<Dns2Result<AddressRecord2>, DnsResult<AddressRecord>>(new(DnsResponseCode.NoError, [new AddressRecord2(IPAddress.IPv6Any, TimeSpan.FromSeconds(2))], TimeSpan.FromSeconds(3)));
    }

    [Fact]
    public void Rfc8484Test()
    {
        const string hostName = "github.com";
        var server = DohConstants.Cloudflare;
        var url = $"{server}?dns={Base64Url.EncodeToString(Encoding.UTF8.GetBytes(hostName))}";

        var req = DohResolver.Rfc8484.GetRequest(new Uri(server, UriKind.Absolute), HttpMethod.Get, hostName);
        Assert.True(req.RequestUri!.OriginalString == url);
    }

    [Fact]
    public void OtherTest()
    {
        var uri1 = new UriBuilder("https://www.baidu.com"[(5 + 3)..]).Uri;
        var host1 = uri1.Host;

        Console.WriteLine(host1);
    }
}
