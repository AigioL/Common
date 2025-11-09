#if !DISABLE_MP2 && !NETFRAMEWORK
using MemoryPack;
#endif
using System.Net;
using System.Text.Json;

namespace AigioL.Common.Models;

#if !DISABLE_MP2 && !NETFRAMEWORK
//#if MP2_GENERATE_TS
//[global::MemoryPack.GenerateTypeScript] 已手动修改生成代码
//#endif
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Sequential)]
#endif
public partial record class ApiRsp
{
    /// <summary>
    /// 是否成功
    /// </summary>
    /// <returns></returns>
    public bool IsSuccess() => (Code >= 200u) && (Code <= 299u);

    /// <summary>
    /// 状态码
    /// </summary>
    public uint Code { get; set; }

    /// <summary>
    /// 附加消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 调用的名称标识，例如请求地址或命令名称
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// https://github.com/dotnet/aspnetcore/blob/v9.0.8/src/Http/Http.Extensions/src/DefaultProblemDetailsWriter.cs#L58
    /// </summary>
    public string? TraceId { get; set; }

    public static implicit operator ApiRsp(bool isSuccess) => isSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest;

    public static implicit operator ApiRsp(HttpStatusCode statusCode) => new() { Code = unchecked((uint)statusCode) };

    public static implicit operator ApiRsp(ApiRspCode code) => new() { Code = unchecked((uint)code) };

    public static implicit operator ApiRsp(string message) => new() { Message = message };

    static ApiRsp Create(ApiRspCode code, string message)
    {
        ApiRsp r = new()
        {
            Code = unchecked((uint)code),
            Message = message
        };
        return r;
    }

    public static implicit operator ApiRsp((ApiRspCode code, string message) t) => Create(t.code, t.message);

    public static implicit operator ApiRsp((string message, ApiRspCode code) t) => Create(t.code, t.message);

    public static implicit operator ApiRsp(Exception exception)
    {
        ApiRsp result = new();
        result.SetException(exception);
        return result;
    }

    public static ApiRsp<TContent> Create<TContent>(TContent content)
    {
        var result = new ApiRsp<TContent> { Content = content, };
        return result;
    }

    public static ApiRsp<TContent> Ok<TContent>(TContent content)
    {
        var result = new ApiRsp<TContent> { Content = content, Code = unchecked((uint)HttpStatusCode.OK), };
        return result;
    }
}

#if !DISABLE_MP2 && !NETFRAMEWORK
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Sequential)]
#endif
public sealed partial record class ApiRsp<TContent> : ApiRsp
{
    /// <summary>
    /// 附加内容
    /// </summary>
    public TContent? Content { get; set; }

    public static implicit operator ApiRsp<TContent>(TContent content) => new() { Content = content, Code = unchecked((uint)HttpStatusCode.OK), };

    public static implicit operator ApiRsp<TContent>(bool isSuccess) => isSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest;

    public static implicit operator ApiRsp<TContent>(HttpStatusCode statusCode) => new() { Code = unchecked((uint)statusCode) };

    public static implicit operator ApiRsp<TContent>(ApiRspCode code) => new() { Code = unchecked((uint)code) };

    public static implicit operator ApiRsp<TContent>(string message) => new() { Message = message };

    static ApiRsp<TContent> Create(ApiRspCode code, string message)
    {
        ApiRsp<TContent> r = new()
        {
            Code = unchecked((uint)code),
            Message = message
        };
        return r;
    }

    public static implicit operator ApiRsp<TContent>((ApiRspCode code, string message) t) => Create(t.code, t.message);

    public static implicit operator ApiRsp<TContent>((string message, ApiRspCode code) t) => Create(t.code, t.message);

    public static implicit operator ApiRsp<TContent>(Exception exception)
    {
        ApiRsp<TContent> result = new();
        result.SetException(exception);
        return result;
    }
}