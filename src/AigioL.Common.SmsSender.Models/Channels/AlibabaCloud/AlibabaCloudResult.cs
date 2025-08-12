using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.SmsSender.Models.Channels.AlibabaCloud;

/// <summary>
/// 提供阿里云 API 请求的返回结果
/// </summary>
/// <typeparam name="T"></typeparam>
public class AlibabaCloudResult<T> : IJsonSerializerContext where T : AlibabaCloudResult<T>
{
    static JsonSerializerContext IJsonSerializerContext.GetDefault() => SmsSenderJsonSerializerContext.Default;

    /// <summary>
    /// 状态码
    /// https://help.aliyun.com/document_detail/55323.html
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 判断结果是否为 "OK"
    /// </summary>
    /// <returns>如果结果为"OK"，返回 <see langword="true"/> ；否则返回 <see langword="false"/></returns>
    public virtual bool IsOK() => Code?.Equals("OK", StringComparison.OrdinalIgnoreCase) ?? false;

    public virtual string GetJsonString(bool writeIndented = false)
    {
        var typeInfo = IJsonSerializerContext.GetJsonSerializerOptions<T>(writeIndented).GetTypeInfo(GetType());
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