using AigioL.Common.SmsSender.Models.Abstractions;
using System.Text.Json.Serialization;

namespace AigioL.Common.SmsSender.Models;

/// <summary>
/// 发送短信的结果类
/// </summary>
public sealed class SendSmsResult : SmsResult<SendSmsResult>, ISendSmsResult, IJsonSerializerContext
{
    static JsonSerializerContext IJsonSerializerContext.GetDefault() => SmsSenderJsonSerializerContext.Default;
}

/// <summary>
/// 发送短信的结果类，带有泛型参数
/// </summary>
/// <typeparam name="TResult">结果的泛型参数类型，必须实现 <see cref="IJsonSerializerContext"/> 接口</typeparam>
public class SendSmsResult<TResult> : SmsResult<TResult, SendSmsResult<TResult>>, ISendSmsResult, IJsonSerializerContext
  where TResult : IJsonSerializerContext
{
    static JsonSerializerContext IJsonSerializerContext.GetDefault() => SmsSenderJsonSerializerContext.Default;
}