#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System;

public sealed class ExitApplicationException : Exception
{
    public ExitApplicationException(int exitCode)
    {
        ExitCode = exitCode;
    }

    public ExitApplicationException(int exitCode, string message) : base(message)
    {
        ExitCode = exitCode;
    }

    public int ExitCode { get; }
}