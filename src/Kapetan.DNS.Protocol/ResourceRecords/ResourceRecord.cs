using DNS.Protocol.Marshalling;
using DNS.Protocol.Utils;
using System.Runtime.InteropServices;

namespace DNS.Protocol.ResourceRecords;

public sealed class ResourceRecord : IResourceRecord
{
    Domain domain;
    RecordType type;
    RecordClass klass;
    TimeSpan ttl;
    ReadOnlyMemory<byte> data;

    public static IList<ResourceRecord> GetAllFromArray(ReadOnlyMemory<byte> message, int offset, int count)
    {
        return GetAllFromArray(message, offset, count, out _);
    }

    public static IList<ResourceRecord> GetAllFromArray(ReadOnlyMemory<byte> message, int offset, int count, out int endOffset)
    {
        var records = new List<ResourceRecord>(count);

        for (int i = 0; i < count; i++)
        {
            records.Add(FromArray(message, offset, out offset));
        }

        endOffset = offset;
        return records;
    }

    public static ResourceRecord FromArray(ReadOnlyMemory<byte> message, int offset)
    {
        return FromArray(message, offset, out _);
    }

    public static ResourceRecord FromArray(ReadOnlyMemory<byte> message, int offset, out int endOffset)
    {
        var domain = Domain.FromArray(message, offset, out offset);
        Span<byte> tailSpan = stackalloc byte[Tail.SIZE];
        ref var tail = ref StructHelper.GetRefStruct<Tail>(message.Span.Slice(offset, tailSpan.Length), tailSpan);

        offset += Tail.SIZE;
        var data = message.Slice(offset, tail.DataLength);

        endOffset = offset + data.Length;

        return new ResourceRecord(domain, data, tail.Type, tail.Class, tail.TimeToLive);
    }

    public static ResourceRecord FromQuestion(Question question, ReadOnlyMemory<byte> data, TimeSpan ttl = default)
    {
        return new ResourceRecord(question.Name, data, question.Type, question.Class, ttl);
    }

    public ResourceRecord(Domain domain, ReadOnlyMemory<byte> data, RecordType type,
            RecordClass klass = RecordClass.IN, TimeSpan ttl = default)
    {
        this.domain = domain;
        this.type = type;
        this.klass = klass;
        this.ttl = ttl;
        this.data = data;
    }

    public Domain Name
    {
        get { return domain; }
    }

    public RecordType Type
    {
        get { return type; }
    }

    public RecordClass Class
    {
        get { return klass; }
    }

    public TimeSpan TimeToLive
    {
        get { return ttl; }
    }

    public int DataLength
    {
        get { return data.Length; }
    }

    public ReadOnlyMemory<byte> Data
    {
        get { return data; }
    }

    public int Size
    {
        get { return domain.Size + Tail.SIZE + data.Length; }
    }

    public void Write(Span<byte> result)
    {
        if (result.Length < Size)
        {
            throw new ArgumentException("Result span is too small");
        }

        domain.Write(result);
        result = result[domain.Size..];
        StructHelper.Write(new Tail
        {
            Type = Type,
            Class = Class,
            TimeToLive = ttl,
            DataLength = data.Length,
        }, result);
        result = result[Tail.SIZE..];
        data.Span.CopyTo(result);
    }

    public override string ToString()
    {
        return ObjectStringifier<ResourceRecord>.New(this)
            .Add(nameof(Name), nameof(Type), nameof(Class), nameof(TimeToLive), nameof(DataLength))
            .ToString();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct Tail : IEndian
    {
        static Endianness IEndian.GetEndianness() => Endianness.Big;

        public const int SIZE = 10;

        ushort type;
        ushort klass;
        uint ttl;
        ushort dataLength;

        public RecordType Type
        {
            get { return (RecordType)type; }
            set { type = (ushort)value; }
        }

        public RecordClass Class
        {
            get { return (RecordClass)klass; }
            set { klass = (ushort)value; }
        }

        public TimeSpan TimeToLive
        {
            get { return TimeSpan.FromSeconds(ttl); }
            set { ttl = (uint)value.TotalSeconds; }
        }

        public int DataLength
        {
            get { return dataLength; }
            set { dataLength = (ushort)value; }
        }
    }
}
