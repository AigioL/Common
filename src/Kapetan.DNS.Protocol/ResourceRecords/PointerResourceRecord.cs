using System.Net;

namespace DNS.Protocol.ResourceRecords;

public sealed class PointerResourceRecord : BaseResourceRecord
{
    public PointerResourceRecord(IResourceRecord record, ReadOnlySpan<byte> message, int dataOffset)
        : base(record)
    {
        PointerDomainName = Domain.FromArray(message, dataOffset);
    }

    public PointerResourceRecord(IPAddress ip, Domain pointer, TimeSpan ttl = default) :
        base(new ResourceRecord(Domain.PointerName(ip), pointer.ToArray(), RecordType.PTR, RecordClass.IN, ttl))
    {
        PointerDomainName = pointer;
    }

    public Domain PointerDomainName { get; }

    public override string ToString()
    {
        return Stringify<PointerResourceRecord>().Add(nameof(PointerDomainName)).ToString();
    }
}
