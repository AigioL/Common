using System.Net;

namespace DNS.Protocol.ResourceRecords;

public sealed class IPAddressResourceRecord : BaseResourceRecord
{
    static IResourceRecord Create(Domain domain, IPAddress ip, TimeSpan ttl)
    {
        byte[] data = ip.GetAddressBytes();
        RecordType type = data.Length == 4 ? RecordType.A : RecordType.AAAA;

        return new ResourceRecord(domain, data, type, RecordClass.IN, ttl);
    }

    public IPAddressResourceRecord(IResourceRecord record) : base(record)
    {
        IPAddress = new IPAddress(Data.Span);
    }

    public IPAddressResourceRecord(Domain domain, IPAddress ip, TimeSpan ttl = default) :
        base(Create(domain, ip, ttl))
    {
        IPAddress = ip;
    }

    public IPAddress IPAddress { get; }

    public override string ToString()
    {
        return Stringify<IPAddressResourceRecord>().Add(nameof(IPAddress)).ToString();
    }
}
