namespace DNS.Protocol.ResourceRecords;

public sealed class MailExchangeResourceRecord : BaseResourceRecord
{
    const int PREFERENCE_SIZE = 2;

    static IResourceRecord Create(Domain domain, int preference, Domain exchange, TimeSpan ttl)
    {
        Span<byte> pref = stackalloc byte[sizeof(ushort)];
        BitConverter.TryWriteBytes(pref, (ushort)preference);
        byte[] data = GC.AllocateUninitializedArray<byte>(pref.Length + exchange.Size);

        if (BitConverter.IsLittleEndian)
        {
            pref.Reverse();
        }

        pref.CopyTo(data);
        exchange.Write(data.AsSpan(pref.Length));

        return new ResourceRecord(domain, data, RecordType.MX, RecordClass.IN, ttl);
    }

    public MailExchangeResourceRecord(IResourceRecord record, ReadOnlySpan<byte> message, int dataOffset)
        : base(record)
    {
        Span<byte> preference = stackalloc byte[PREFERENCE_SIZE];
        message.Slice(dataOffset, PREFERENCE_SIZE).CopyTo(preference);

        if (BitConverter.IsLittleEndian)
        {
            preference.Reverse();
        }

        dataOffset += PREFERENCE_SIZE;

        Preference = BitConverter.ToUInt16(preference);
        ExchangeDomainName = Domain.FromArray(message, dataOffset);
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
