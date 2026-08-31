namespace DNS.Protocol.ResourceRecords;

public sealed class NameServerResourceRecord : BaseResourceRecord
{
    public NameServerResourceRecord(IResourceRecord record, ReadOnlyMemory<byte> message, int dataOffset)
        : base()
    {
        NSDomainName = Domain.FromArray(message, dataOffset, out var endOffset);

        // If encoded length differs from expanded domain size, RDATA used a compression pointer.
        // Rebuild data to avoid stale pointer offsets when re-serializing (issue #73).
        // https://github.com/kapetan/dns/issues/73
        if (endOffset - dataOffset != NSDomainName.Size)
        {
            this.record = new ResourceRecord(record.Name, NSDomainName.ToArray(), RecordType.NS, record.Class, record.TimeToLive);
        }
        else
        {
            this.record = record;
        }
    }

    public NameServerResourceRecord(Domain domain, Domain nsDomain, TimeSpan ttl = default) :
        base(new ResourceRecord(domain, nsDomain.ToArray(), RecordType.NS, RecordClass.IN, ttl))
    {
        NSDomainName = nsDomain;
    }

    public Domain NSDomainName { get; }

    public override string ToString()
    {
        return Stringify<NameServerResourceRecord>().Add(nameof(NSDomainName)).ToString();
    }
}
