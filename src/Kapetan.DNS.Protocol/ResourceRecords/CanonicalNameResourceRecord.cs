namespace DNS.Protocol.ResourceRecords;

public sealed class CanonicalNameResourceRecord : BaseResourceRecord
{
    public CanonicalNameResourceRecord(IResourceRecord record, ReadOnlyMemory<byte> message, int dataOffset)
        : base()
    {
        CanonicalDomainName = Domain.FromArray(message, dataOffset, out var endOffset);

        // If encoded length differs from expanded domain size, RDATA used a compression pointer.
        // Rebuild data to avoid stale pointer offsets when re-serializing (issue #73).
        // https://github.com/kapetan/dns/issues/73
        if (endOffset - dataOffset != CanonicalDomainName.Size)
        {
            this.record = new ResourceRecord(record.Name, CanonicalDomainName.ToArray(), RecordType.CNAME, record.Class, record.TimeToLive);
        }
        else
        {
            this.record = record;
        }
    }

    public CanonicalNameResourceRecord(Domain domain, Domain cname, TimeSpan ttl = default) :
        base(new ResourceRecord(domain, cname.ToArray(), RecordType.CNAME, RecordClass.IN, ttl))
    {
        CanonicalDomainName = cname;
    }

    public Domain CanonicalDomainName { get; }

    public override string ToString()
    {
        return Stringify<CanonicalNameResourceRecord>().Add(nameof(CanonicalDomainName)).ToString();
    }
}
