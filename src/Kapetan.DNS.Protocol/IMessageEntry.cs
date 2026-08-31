namespace DNS.Protocol;

public interface IMessageEntry
{
    Domain Name { get; }

    RecordType Type { get; }

    RecordClass Class { get; }

    int Size { get; }

    void Write(Span<byte> result);
}
