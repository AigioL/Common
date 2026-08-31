namespace DNS.Protocol.ResourceRecords;

public sealed class NameServerResourceRecord : BaseResourceRecord
{
    public NameServerResourceRecord(IResourceRecord record, ReadOnlyMemory<byte> message, int dataOffset)
        : base(record)
    {
        NSDomainName = Domain.FromArray(message, dataOffset);
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
