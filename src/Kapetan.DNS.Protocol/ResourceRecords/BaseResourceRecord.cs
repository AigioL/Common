using DNS.Protocol.Utils;
using System.Diagnostics.CodeAnalysis;

namespace DNS.Protocol.ResourceRecords;

public abstract class BaseResourceRecord : IResourceRecord
{
    private IResourceRecord record;

    public BaseResourceRecord(IResourceRecord record)
    {
        this.record = record;
    }

    public Domain Name
    {
        get { return record.Name; }
    }

    public RecordType Type
    {
        get { return record.Type; }
    }

    public RecordClass Class
    {
        get { return record.Class; }
    }

    public TimeSpan TimeToLive
    {
        get { return record.TimeToLive; }
    }

    public int DataLength
    {
        get { return record.DataLength; }
    }

    public ReadOnlyMemory<byte> Data
    {
        get { return record.Data; }
    }

    public int Size
    {
        get { return record.Size; }
    }

    public void Write(Span<byte> result)
    {
        record.Write(result);
    }

    internal ObjectStringifier<T> Stringify<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>()
    {
        return ObjectStringifier<T>.New(this)
            .Add(nameof(Name), nameof(Type), nameof(Class), nameof(TimeToLive), nameof(DataLength));
    }
}
