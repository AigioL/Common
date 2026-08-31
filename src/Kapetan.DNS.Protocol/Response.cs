using DNS.Protocol.ResourceRecords;
using DNS.Protocol.Utils;
using System.Net;

namespace DNS.Protocol;

public sealed class Response : IResponse
{
    Header header;
    IList<Question> questions;
    IList<IResourceRecord> answers;
    IList<IResourceRecord> authority;
    IList<IResourceRecord> additional;

    public static Response FromRequest(IRequest request)
    {
        Response response = new()
        {
            Id = request.Id,
        };

        foreach (var question in request.Questions)
        {
            response.Questions.Add(question);
        }

        return response;
    }

    public static Response FromArray(ReadOnlyMemory<byte> message)
    {
        var header = Header.FromArray(message.Span);
        int offset = header.Size;

        if (!header.Response)
        {
            throw new ArgumentException("Invalid response message");
        }

        if (header.Truncated)
        {
            return new Response(header,
                Question.GetAllFromArray(message, offset, header.QuestionCount),
                new List<IResourceRecord>(),
                new List<IResourceRecord>(),
                new List<IResourceRecord>());
        }

        return new Response(header,
            Question.GetAllFromArray(message, offset, header.QuestionCount, out offset),
            ResourceRecordFactory.GetAllFromArray(message, offset, header.AnswerRecordCount, out offset),
            ResourceRecordFactory.GetAllFromArray(message, offset, header.AuthorityRecordCount, out offset),
            ResourceRecordFactory.GetAllFromArray(message, offset, header.AdditionalRecordCount, out offset));
    }

    public Response(Header header, IList<Question> questions, IList<IResourceRecord> answers,
            IList<IResourceRecord> authority, IList<IResourceRecord> additional)
    {
        this.header = header;
        this.questions = questions;
        this.answers = answers;
        this.authority = authority;
        this.additional = additional;
    }

    public Response()
    {
        header = new Header();
        questions = new List<Question>();
        answers = new List<IResourceRecord>();
        authority = new List<IResourceRecord>();
        additional = new List<IResourceRecord>();

        header.Response = true;
    }

    public Response(IResponse response)
    {
        header = new Header();
        questions = [.. response.Questions];
        answers = [.. response.AnswerRecords];
        authority = [.. response.AuthorityRecords];
        additional = [.. response.AdditionalRecords];

        header.Response = true;

        Id = response.Id;
        RecursionAvailable = response.RecursionAvailable;
        AuthorativeServer = response.AuthorativeServer;
        OperationCode = response.OperationCode;
        ResponseCode = response.ResponseCode;
    }

    public IList<Question> Questions
    {
        get { return questions; }
    }

    public IList<IResourceRecord> AnswerRecords
    {
        get { return answers; }
    }

    public IList<IResourceRecord> AuthorityRecords
    {
        get { return authority; }
    }

    public IList<IResourceRecord> AdditionalRecords
    {
        get { return additional; }
    }

    public int Id
    {
        get { return header.Id; }
        set { header.Id = value; }
    }

    public bool RecursionAvailable
    {
        get { return header.RecursionAvailable; }
        set { header.RecursionAvailable = value; }
    }

    public bool AuthenticData
    {
        get { return header.AuthenticData; }
        set { header.AuthenticData = value; }
    }

    public bool CheckingDisabled
    {
        get { return header.CheckingDisabled; }
        set { header.CheckingDisabled = value; }
    }

    public bool AuthorativeServer
    {
        get { return header.AuthorativeServer; }
        set { header.AuthorativeServer = value; }
    }

    public bool Truncated
    {
        get { return header.Truncated; }
        set { header.Truncated = value; }
    }

    public OperationCode OperationCode
    {
        get { return header.OperationCode; }
        set { header.OperationCode = value; }
    }

    public DnsResponseCode ResponseCode
    {
        get { return header.ResponseCode; }
        set { header.ResponseCode = value; }
    }

    public int Size
    {
        get
        {
            return header.Size +
                questions.Sum(q => q.Size) +
                answers.Sum(a => a.Size) +
                authority.Sum(a => a.Size) +
                additional.Sum(a => a.Size);
        }
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
        foreach (var it in answers)
        {
            it.Write(result);
            result = result[it.Size..];
        }
        foreach (var it in authority)
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

        return ObjectStringifier<Response>.New(this)
            .Add(nameof(Header), header)
            .Add(nameof(Questions), nameof(AnswerRecords), nameof(AuthorityRecords), nameof(AdditionalRecords))
            .ToString();
    }

    void UpdateHeader()
    {
        header.QuestionCount = questions.Count;
        header.AnswerRecordCount = answers.Count;
        header.AuthorityRecordCount = authority.Count;
        header.AdditionalRecordCount = additional.Count;
    }
}
