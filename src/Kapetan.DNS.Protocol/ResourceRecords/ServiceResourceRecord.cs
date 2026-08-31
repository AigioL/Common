using DNS.Protocol.Marshalling;
using System.Runtime.InteropServices;

namespace DNS.Protocol.ResourceRecords;

public sealed class ServiceResourceRecord : BaseResourceRecord
{
    static IResourceRecord Create(Domain domain, ushort priority, ushort weight, ushort port, Domain target, TimeSpan ttl)
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

        return new ResourceRecord(domain, data, RecordType.SRV, RecordClass.IN, ttl);
    }

    public ServiceResourceRecord(IResourceRecord record, ReadOnlySpan<byte> message, int dataOffset) : base(record)
    {
        Head head = StructHelper.GetStruct<Head>(message.Slice(dataOffset, Head.SIZE));

        Priority = head.Priority;
        Weight = head.Weight;
        Port = head.Port;
        Target = Domain.FromArray(message, dataOffset + Head.SIZE);
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
