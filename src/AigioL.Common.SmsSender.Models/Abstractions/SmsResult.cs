using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.SmsSender.Models.Abstractions;

/// <summary>
/// 表示短信结果的抽象类
/// </summary>
/// <typeparam name="TImplement">实现类的类型，必须是 <see cref="SmsResult{TImplement}"/> 或其派生类</typeparam>
public abstract class SmsResult<TImplement>
    where TImplement : SmsResult<TImplement>, IJsonSerializerContext
{
    /// <summary>
    /// 获取或设置短信操作是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 获取或设置短信操作的结果信息
    /// </summary>
    public ISmsSubResult? Result { get; set; }

    /// <summary>
    /// 获取或设置短信操作的 HTTP 状态码
    /// </summary>
    public int HttpStatusCode { get; set; }

    public virtual string GetJsonString(bool writeIndented = false)
    {
        var typeInfo = IJsonSerializerContext.GetJsonSerializerOptions<TImplement>(writeIndented).GetTypeInfo(GetType());
        ArgumentNullException.ThrowIfNull(typeInfo);
        var json = JsonSerializer.Serialize(this, typeInfo);
        return json;
    }

    public override string? ToString()
    {
        try
        {
            var json = GetJsonString(true);
            return json;
        }
        catch
        {
            var str = base.ToString();
            return str;
        }
    }
}

/// <summary>
/// 表示短信结果的泛型抽象类
/// </summary>
/// <typeparam name="TResult">结果的泛型参数类型，必须实现 <see cref="IJsonSerializerContext"/> 接口</typeparam>
/// <typeparam name="TImplement">实现类的类型，必须是 <see cref="SmsResult{TResult, TImplement}"/> 或其派生类</typeparam>
public abstract class SmsResult<TResult, TImplement> : SmsResult<TImplement>, ISmsResult
   where TResult : IJsonSerializerContext
   where TImplement : SmsResult<TResult, TImplement>, IJsonSerializerContext
{
    /// <summary>
    /// 获取或设置短信操作的具体结果
    /// </summary>
    public new TResult? Result { get; set; }

    /// <summary>
    /// 获取或设置短信操作的结果对象
    /// </summary>
    public ISmsSubResult? ResultObject { get => base.Result; set => base.Result = value; }

    /// <summary>
    /// 将 <see cref="Result"/> 属性转换为 <see cref="ResultObject"/> 对象，并使用 <see cref="ISmsSubResult"/> 类型进行接收
    /// </summary>
    ISmsSubResult? ISmsResult.Result => ResultObject;
}