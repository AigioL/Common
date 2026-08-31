using System.Collections.Generic;

namespace DNS.Protocol;

public interface IMessage
{
    IList<Question> Questions { get; }

    int Size { get; }

    void Write(Span<byte> result);
}
