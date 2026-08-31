using System.Net;

namespace DNS.Protocol.ResourceRecords;

public sealed class PointerResourceRecord : BaseResourceRecord
{
    public PointerResourceRecord(IResourceRecord record, ReadOnlyMemory<byte> message, int dataOffset)
        : base()
    {
        PointerDomainName = Domain.FromArray(message, dataOffset, out var endOffset);

        // If encoded length differs from expanded domain size, RDATA used a compression pointer.
        // Rebuild data to avoid stale pointer offsets when re-serializing (issue #73).
        // https://github.com/kapetan/dns/issues/73
        if (endOffset - dataOffset != PointerDomainName.Size)
        {
            this.record = new ResourceRecord(record.Name, PointerDomainName.ToArray(), RecordType.PTR, record.Class, record.TimeToLive);
        }
        else
        {
            this.record = record;
        }
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
