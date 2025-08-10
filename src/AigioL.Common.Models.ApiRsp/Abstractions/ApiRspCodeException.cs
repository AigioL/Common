namespace AigioL.Common.Models.Abstractions;

public abstract partial class ApiRspCodeException : ApplicationException
{
    public abstract uint GetCode();

    public ApiRspCodeException(string? message) : base(message)
    {
    }

    public ApiRspCodeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
