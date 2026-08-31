using DNS.Protocol.Marshalling;

namespace DNS.Protocol.Marshalling;

public interface IEndian
{
    static abstract Endianness GetEndianness();
}
