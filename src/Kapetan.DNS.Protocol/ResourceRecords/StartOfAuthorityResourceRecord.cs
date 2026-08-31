using DNS.Protocol.Marshalling;
using System.Runtime.InteropServices;

namespace DNS.Protocol.ResourceRecords;

public sealed class StartOfAuthorityResourceRecord : BaseResourceRecord
{
    static IResourceRecord Create(Domain domain, Domain master, Domain responsible, long serial,
        TimeSpan refresh, TimeSpan retry, TimeSpan expire, TimeSpan minTtl, TimeSpan ttl)
    {
        byte[] data = GC.AllocateUninitializedArray<byte>(Options.SIZE + master.Size + responsible.Size);
        Options tail = new Options()
        {
            SerialNumber = serial,
            RefreshInterval = refresh,
            RetryInterval = retry,
            ExpireInterval = expire,
            MinimumTimeToLive = minTtl,
        };

        Span<byte> temp = data;
        master.Write(temp);
        temp = temp[master.Size..];
        responsible.Write(temp);
        temp = temp[responsible.Size..];
        StructHelper.Write(tail, temp);

        return new ResourceRecord(domain, data, RecordType.SOA, RecordClass.IN, ttl);
    }

    public StartOfAuthorityResourceRecord(IResourceRecord record, ReadOnlySpan<byte> message, int dataOffset)
        : base(record)
    {
        MasterDomainName = Domain.FromArray(message, dataOffset, out dataOffset);
        ResponsibleDomainName = Domain.FromArray(message, dataOffset, out dataOffset);

        Options tail = StructHelper.GetStruct<Options>(message.Slice(dataOffset, Options.SIZE));

        SerialNumber = tail.SerialNumber;
        RefreshInterval = tail.RefreshInterval;
        RetryInterval = tail.RetryInterval;
        ExpireInterval = tail.ExpireInterval;
        MinimumTimeToLive = tail.MinimumTimeToLive;
    }

    public StartOfAuthorityResourceRecord(Domain domain, Domain master, Domain responsible, long serial,
            TimeSpan refresh, TimeSpan retry, TimeSpan expire, TimeSpan minTtl, TimeSpan ttl = default) :
        base(Create(domain, master, responsible, serial, refresh, retry, expire, minTtl, ttl))
    {
        MasterDomainName = master;
        ResponsibleDomainName = responsible;

        SerialNumber = serial;
        RefreshInterval = refresh;
        RetryInterval = retry;
        ExpireInterval = expire;
        MinimumTimeToLive = minTtl;
    }

    public StartOfAuthorityResourceRecord(Domain domain, Domain master, Domain responsible,
            Options options = default, TimeSpan ttl = default) :
        this(domain, master, responsible, options.SerialNumber, options.RefreshInterval, options.RetryInterval,
                options.ExpireInterval, options.MinimumTimeToLive, ttl)
    { }

    public Domain MasterDomainName { get; }

    public Domain ResponsibleDomainName { get; }

    public long SerialNumber { get; }

    public TimeSpan RefreshInterval { get; }

    public TimeSpan RetryInterval { get; }

    public TimeSpan ExpireInterval { get; }

    public TimeSpan MinimumTimeToLive { get; }

    public override string ToString()
    {
        return Stringify<StartOfAuthorityResourceRecord>().Add(nameof(MasterDomainName), nameof(ResponsibleDomainName), nameof(SerialNumber)).ToString();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Options : IEndian
    {
        static Endianness IEndian.GetEndianness() => Endianness.Big;

        public const int SIZE = 20;

        uint serialNumber;
        uint refreshInterval;
        uint retryInterval;
        uint expireInterval;
        uint ttl;

        public long SerialNumber
        {
            get { return serialNumber; }
            set { serialNumber = (uint)value; }
        }

        public TimeSpan RefreshInterval
        {
            get { return TimeSpan.FromSeconds(refreshInterval); }
            set { refreshInterval = (uint)value.TotalSeconds; }
        }

        public TimeSpan RetryInterval
        {
            get { return TimeSpan.FromSeconds(retryInterval); }
            set { retryInterval = (uint)value.TotalSeconds; }
        }

        public TimeSpan ExpireInterval
        {
            get { return TimeSpan.FromSeconds(expireInterval); }
            set { expireInterval = (uint)value.TotalSeconds; }
        }

        public TimeSpan MinimumTimeToLive
        {
            get { return TimeSpan.FromSeconds(ttl); }
            set { ttl = (uint)value.TotalSeconds; }
        }
    }
}
