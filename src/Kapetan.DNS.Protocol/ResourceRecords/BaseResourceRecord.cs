using DNS.Protocol.Utils;
using System.Diagnostics.CodeAnalysis;

namespace DNS.Protocol.ResourceRecords;

public abstract class BaseResourceRecord : IResourceRecord
{
    protected IResourceRecord record = null!;

    protected BaseResourceRecord()
    {
    }

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

    public virtual int DataLength
    {
        get { return record.DataLength; }
    }

    public virtual ReadOnlyMemory<byte> Data
    {
        get { return record.Data; }
    }

    public virtual int Size
    {
        get { return record.Size; }
    }

    public virtual void Write(Span<byte> result)
    {
        record.Write(result);
    }

    internal ObjectStringifier<T> Stringify<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>()
    {
        return ObjectStringifier<T>.New(this)
            .Add(nameof(Name), nameof(Type), nameof(Class), nameof(TimeToLive), nameof(DataLength));
    }
}
