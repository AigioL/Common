using System.Collections.Generic;

namespace DNS.Protocol;

public interface IMessage
{
    IList<Question> Questions { get; }

    int Size { get; }

    [Obsolete("use Write(Span<byte>) instead", true)]
    byte[] ToArray();

    void Write(Span<byte> result);
}
