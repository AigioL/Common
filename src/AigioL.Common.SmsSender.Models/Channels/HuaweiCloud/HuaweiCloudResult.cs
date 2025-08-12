using AigioL.Common.SmsSender.Models.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.SmsSender.Models.Channels.HuaweiCloud;

/// <summary>
/// 提供华为云 API 请求的返回结果
/// </summary>
/// <typeparam name="T"></typeparam>
public class HuaweiCloudResult<T> : IJsonSerializerContext, ISmsSubResult where T : HuaweiCloudResult<T>
{
    static JsonSerializerContext IJsonSerializerContext.GetDefault() => SmsSenderJsonSerializerContext.Default;

    /// <summary>
    /// 状态码
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// 状态描述
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 判断返回结果是否成功
    /// </summary>
    public virtual bool IsOK() => Description?.Equals("Success", StringComparison.OrdinalIgnoreCase) ?? false;

    /// <summary>
    /// 返回包含状态码，状态描述属性值的信息
    /// </summary>
    /// <returns></returns>
    protected virtual string? GetRecord() => $"code: {Code}, description {Description}";

    /// <inheritdoc />
    string? ISmsSubResult.GetRecord() => GetRecord();

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
