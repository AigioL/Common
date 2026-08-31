using DNS.Protocol.Marshalling;
using System.Runtime.InteropServices;

namespace DNS.Protocol.ResourceRecords;

public sealed class ServiceResourceRecord : BaseResourceRecord
{
    static byte[] CreateData(ushort priority, ushort weight, ushort port, Domain target)
    {
        byte[] data = GC.AllocateUninitializedArray<byte>(Head.SIZE + target.Size);

        Head head = new Head()
        {
            Priority = priority,
            Weight = weight,
            Port = port
        };

        StructHelper.Write(head, data);
        target.Write(data.AsSpan(Head.SIZE));
        return data;
    }

    static IResourceRecord Create(Domain domain, ushort priority, ushort weight, ushort port, Domain target, TimeSpan ttl)
    {
        byte[] data = CreateData(priority, weight, port, target);
        return new ResourceRecord(domain, data, RecordType.SRV, RecordClass.IN, ttl);
    }

    public ServiceResourceRecord(IResourceRecord record, ReadOnlyMemory<byte> message, int dataOffset) : base()
    {
        Span<byte> head_buffer = stackalloc byte[Head.SIZE];
        ref var head = ref StructHelper.GetRefStruct<Head>(message.Span.Slice(dataOffset, Head.SIZE), head_buffer);

        Priority = head.Priority;
        Weight = head.Weight;
        Port = head.Port;
        dataOffset += Head.SIZE;
        Target = Domain.FromArray(message, dataOffset, out var endOffset);

        // SRV RDATA = fixed head + target domain. If consumed bytes differ from expanded domain size,
        // target name was pointer-compressed. Rebuild RDATA to avoid stale offsets (issue #73).
        // https://github.com/kapetan/dns/issues/73
        if (endOffset - dataOffset != Target.Size)
        {
            this.record = new ResourceRecord(record.Name, CreateData(Priority, Weight, Port, Target), RecordType.SRV, record.Class, record.TimeToLive);
        }
        else
        {
            this.record = record;
        }
    }

    public ServiceResourceRecord(Domain domain, ushort priority, ushort weight, ushort port, Domain target, TimeSpan ttl = default) :
            base(Create(domain, priority, weight, port, target, ttl))
    {
        Priority = priority;
        Weight = weight;
        Port = port;
        Target = target;
    }

    public ushort Priority { get; }

    public ushort Weight { get; }

    public ushort Port { get; }

    public Domain Target { get; }

    public override string ToString()
    {
        return Stringify<ServiceResourceRecord>().Add(nameof(Priority), nameof(Weight), nameof(Port), nameof(Target)).ToString();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct Head : IEndian
    {
        static Endianness IEndian.GetEndianness() => Endianness.Big;

        public const int SIZE = 6;

        ushort priority;
        ushort weight;
        ushort port;

        public ushort Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public ushort Weight
        {
            get { return weight; }
            set { weight = value; }
        }

        public ushort Port
        {
            get { return port; }
            set { port = value; }
        }
    }
}
