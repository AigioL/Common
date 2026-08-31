using DNS.Protocol.Marshalling;
using DNS.Protocol.Utils;
using System.Runtime.InteropServices;

namespace DNS.Protocol;

public sealed class Question : IMessageEntry
{
    public static IList<Question> GetAllFromArray(ReadOnlySpan<byte> message, int offset, int questionCount)
    {
        return GetAllFromArray(message, offset, questionCount, out _);
    }

    public static IList<Question> GetAllFromArray(ReadOnlySpan<byte> message, int offset, int questionCount, out int endOffset)
    {
        var questions = new List<Question>(questionCount);

        for (int i = 0; i < questionCount; i++)
        {
            questions.Add(FromArray(message, offset, out offset));
        }

        endOffset = offset;
        return questions;
    }

    public static Question FromArray(ReadOnlySpan<byte> message, int offset)
    {
        return FromArray(message, offset, out _);
    }

    public static Question FromArray(ReadOnlySpan<byte> message, int offset, out int endOffset)
    {
        Domain domain = Domain.FromArray(message, offset, out offset);
        Tail tail = StructHelper.GetStruct<Tail>(message.Slice(offset, Tail.SIZE));

        endOffset = offset + Tail.SIZE;

        return new Question(domain, tail.Type, tail.Class);
    }

    Domain domain;
    RecordType type;
    RecordClass klass;

    public Question(Domain domain, RecordType type = RecordType.A, RecordClass klass = RecordClass.IN)
    {
        this.domain = domain;
        this.type = type;
        this.klass = klass;
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

    public int Size
    {
        get { return domain.Size + Tail.SIZE; }
    }

    [Obsolete("use Write(Span<byte>) instead", true)]
    public byte[] ToArray()
    {
        ByteStream result = new ByteStream(Size);

        result
            .Append(domain.ToArray())
            .Append(StructHelper.GetBytes(new Tail { Type = Type, Class = Class }));

        return result.ToArray();
    }

    public void Write(Span<byte> result)
    {
        if (result.Length < Size)
        {
            throw new ArgumentException("Result span is too small");
        }

        domain.Write(result);
        result = result[domain.Size..];
        StructHelper.Write(new Tail { Type = Type, Class = Class }, result);
    }

    public override string ToString()
    {
        return ObjectStringifier<Question>.New(this)
            .Add(nameof(Name), nameof(Type), nameof(Class))
            .ToString();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct Tail : IEndian
    {
        static Endianness IEndian.GetEndianness() => Endianness.Big;

        public const int SIZE = 4;

        ushort type;
        ushort klass;

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
    }
}
