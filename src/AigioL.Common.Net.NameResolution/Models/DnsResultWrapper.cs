using System.Diagnostics;
using System.Net;

namespace AigioL.Common.Net.NameResolution.Models;

/// <summary>
/// DNS 返回结果包装结构
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly record struct DnsResultWrapper<T>
{
    /// <summary>
    /// DNS 返回结果
    /// </summary>
    public DnsResult<T> Result { get; }

    /// <summary>
    /// 结果的来源类型
    /// </summary>
    public DnsResultSourceType SourceType { get; }

    /// <summary>
    /// 用于跟踪的可选字段
    /// </summary>
    public string? TraceId { get; }

    /// <summary>
    /// DNS 返回成功结果耗时
    /// </summary>
    public TimeSpan? ElapsedTime { get; }

    public DnsResultWrapper(DnsResultSourceType sourceType, DnsResult<T> result, string? traceId = null, TimeSpan? elapsedTime = null)
    {
        Result = result;
        SourceType = sourceType;
        TraceId = traceId;
        ElapsedTime = elapsedTime;
    }

    public static implicit operator DnsResultWrapper<T>(DnsResponseCode responseCode)
    {
        Dns2Result<T> result = responseCode;
        return result.ToWrapper(DnsResultSourceType.FixedValue);
    }

    public static implicit operator DnsResult<T>(DnsResultWrapper<T> resultWrapper) => resultWrapper.Result;
}
