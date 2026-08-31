using DNS.Protocol.ResourceRecords;
using DNS.Protocol.Utils;
using System.Net;
using System.Security.Cryptography;

namespace DNS.Protocol;

public sealed class Request : IRequest
{
    static readonly RandomNumberGenerator RANDOM = RandomNumberGenerator.Create();
    IList<Question> questions;
    Header header;
    IList<IResourceRecord> additional;

    [Obsolete("use FromArray(ReadOnlySpan<byte>) instead", true)]
    public static Request FromArray(byte[] message)
    {
        Header header = Header.FromArray(message);
        int offset = header.Size;

        if (header.Response || header.QuestionCount == 0 ||
                header.AnswerRecordCount + header.AuthorityRecordCount > 0 ||
                header.ResponseCode != DnsResponseCode.NoError)
        {
            throw new ArgumentException("Invalid request message");
        }

        return new Request(header,
            Question.GetAllFromArray(message, offset, header.QuestionCount, out offset),
            ResourceRecordFactory.GetAllFromArray(message, offset, header.AdditionalRecordCount, out _));
    }

    public static Request FromArray(ReadOnlySpan<byte> message)
    {
        Header header = Header.FromArray(message);
        int offset = header.Size;

        if (header.Response || header.QuestionCount == 0 ||
                header.AnswerRecordCount + header.AuthorityRecordCount > 0 ||
                header.ResponseCode != DnsResponseCode.NoError)
        {
            throw new ArgumentException("Invalid request message");
        }

        return new Request(header,
            Question.GetAllFromArray(message, offset, header.QuestionCount, out offset),
            ResourceRecordFactory.GetAllFromArray(message, offset, header.AdditionalRecordCount, out _));
    }

    public Request(Header header, IList<Question> questions, IList<IResourceRecord> additional)
    {
        this.header = header;
        this.questions = questions;
        this.additional = additional;
    }

    public Request()
    {
        questions = new List<Question>();
        header = new Header();
        additional = new List<IResourceRecord>();

        header.OperationCode = OperationCode.Query;
        header.Response = false;
        header.Id = NextRandomId();
    }

    public Request(IRequest request)
    {
        header = new Header();
        questions = [.. request.Questions];
        additional = [.. request.AdditionalRecords];

        header.Response = false;

        Id = request.Id;
        OperationCode = request.OperationCode;
        RecursionDesired = request.RecursionDesired;
    }

    public IList<Question> Questions
    {
        get { return questions; }
    }

    public IList<IResourceRecord> AdditionalRecords
    {
        get { return additional; }
    }

    public int Size
    {
        get
        {
            return header.Size +
                questions.Sum(q => q.Size) +
                additional.Sum(a => a.Size);
        }
    }

    public int Id
    {
        get { return header.Id; }
        set { header.Id = value; }
    }

    public OperationCode OperationCode
    {
        get { return header.OperationCode; }
        set { header.OperationCode = value; }
    }

    public bool RecursionDesired
    {
        get { return header.RecursionDesired; }
        set { header.RecursionDesired = value; }
    }

    [Obsolete("use Write(Span<byte>) instead", true)]
    public byte[] ToArray()
    {
        UpdateHeader();
        ByteStream result = new ByteStream(Size);

        result
            .Append(header.ToArray())
            .Append(questions.Select(q => q.ToArray()))
            .Append(additional.Select(a => a.ToArray()));

        return result.ToArray();
    }

    public void Write(Span<byte> result)
    {
        if (result.Length < Size)
        {
            throw new ArgumentException("Result span is too small");
        }
        UpdateHeader();
        header.Write(result);
        result = result[header.Size..];
        foreach (var it in questions)
        {
            it.Write(result);
            result = result[it.Size..];
        }
        foreach (var it in additional)
        {
            it.Write(result);
            result = result[it.Size..];
        }
    }

    public override string ToString()
    {
        UpdateHeader();

        return ObjectStringifier<Request>.New(this)
            .Add(nameof(Header), header)
            .Add(nameof(Questions), nameof(AdditionalRecords))
            .ToString();
    }

    private void UpdateHeader()
    {
        header.QuestionCount = questions.Count;
        header.AdditionalRecordCount = additional.Count;
    }

    private ushort NextRandomId()
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        RANDOM.GetBytes(buffer);
        return BitConverter.ToUInt16(buffer);
    }
}
