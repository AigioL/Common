using AigioL.Common.SmsSender.Models.Abstractions;
using System.Text.Json.Serialization;

namespace AigioL.Common.SmsSender.Models;

/// <summary>
/// 验证短信结果的类
/// </summary>
public sealed class CheckSmsResult : SmsResult<CheckSmsResult>, ICheckSmsResult, IJsonSerializerContext
{
    static JsonSerializerContext IJsonSerializerContext.GetDefault() => SmsSenderJsonSerializerContext.Default;

    /// <inheritdoc/>
    public bool IsCheckSuccess { get; set; }
}

/// <summary>
/// 验证短信结果的类，带有泛型参数
/// </summary>
/// <typeparam name="TResult">结果的泛型参数类型，必须实现 <see cref="IJsonSerializerContext"/> 接口 </typeparam>
public class CheckSmsResult<TResult> : SmsResult<TResult, CheckSmsResult<TResult>>, ICheckSmsResult, IJsonSerializerContext
  where TResult : IJsonSerializerContext
{
    static JsonSerializerContext IJsonSerializerContext.GetDefault() => SmsSenderJsonSerializerContext.Default;

    /// <inheritdoc/>
    public bool IsCheckSuccess { get; set; }
}