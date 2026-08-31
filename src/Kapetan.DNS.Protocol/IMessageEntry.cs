namespace DNS.Protocol;

public interface IMessageEntry
{
    Domain Name { get; }

    RecordType Type { get; }

    RecordClass Class { get; }

    int Size { get; }

    [Obsolete("use Write(Span<byte>) instead", true)]
    byte[] ToArray();

    void Write(Span<byte> result);
}
