using System.Buffers.Binary;

namespace DNS.Protocol.ResourceRecords;

public sealed class MailExchangeResourceRecord : BaseResourceRecord
{
    const int PREFERENCE_SIZE = sizeof(ushort);

    static byte[] CreateData(int preference, Domain exchange)
    {
        Span<byte> pref = stackalloc byte[PREFERENCE_SIZE];
        BinaryPrimitives.WriteUInt16BigEndian(pref, (ushort)preference);
        byte[] data = GC.AllocateUninitializedArray<byte>(pref.Length + exchange.Size);

        pref.CopyTo(data);
        exchange.Write(data.AsSpan(pref.Length));
        return data;
    }

    static IResourceRecord Create(Domain domain, int preference, Domain exchange, TimeSpan ttl)
    {
        byte[] data = CreateData(preference, exchange);
        return new ResourceRecord(domain, data, RecordType.MX, RecordClass.IN, ttl);
    }

    public MailExchangeResourceRecord(IResourceRecord record, ReadOnlyMemory<byte> message, int dataOffset)
        : base()
    {
        Preference = BinaryPrimitives.ReadUInt16BigEndian(message.Span.Slice(dataOffset, PREFERENCE_SIZE));
        dataOffset += PREFERENCE_SIZE;

        ExchangeDomainName = Domain.FromArray(message, dataOffset, out var endOffset);

        // MX RDATA = preference(2) + exchange domain. If consumed bytes differ from expanded domain size,
        // exchange name was pointer-compressed. Rebuild RDATA to avoid stale offsets (issue #73).
        // https://github.com/kapetan/dns/issues/73
        if (endOffset - dataOffset != ExchangeDomainName.Size)
        {
            this.record = new ResourceRecord(record.Name, CreateData(Preference, ExchangeDomainName), RecordType.MX, record.Class, record.TimeToLive);
        }
        else
        {
            this.record = record;
        }
    }

    public MailExchangeResourceRecord(Domain domain, int preference, Domain exchange, TimeSpan ttl = default) :
        base(Create(domain, preference, exchange, ttl))
    {
        Preference = preference;
        ExchangeDomainName = exchange;
    }

    public int Preference { get; }

    public Domain ExchangeDomainName { get; }

    public override string ToString()
    {
        return Stringify<MailExchangeResourceRecord>().Add(nameof(Preference), nameof(ExchangeDomainName)).ToString();
    }
}
