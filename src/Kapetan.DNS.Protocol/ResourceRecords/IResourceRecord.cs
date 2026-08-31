namespace DNS.Protocol.ResourceRecords;

public interface IResourceRecord : IMessageEntry
{
    TimeSpan TimeToLive { get; }

    int DataLength { get; }

    ReadOnlyMemory<byte> Data { get; }
}
