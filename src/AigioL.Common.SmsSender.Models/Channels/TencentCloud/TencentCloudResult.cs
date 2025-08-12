using AigioL.Common.SmsSender.Models.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.SmsSender.Models.Channels.TencentCloud;

public class TencentCloudResult<T> : ISmsSubResult, IJsonSerializerContext where T : ITencentCloud, IJsonSerializerContext
{
    static JsonSerializerContext IJsonSerializerContext.GetDefault() => SmsSenderJsonSerializerContext.Default;

    [global::System.Text.Json.Serialization.JsonPropertyName("Response")]
    //[global::Newtonsoft.Json.JsonProperty("Response")]
    public required T Response { get; set; }

    /// <summary>
    /// 返回包含了状态、消息内容、 唯一标识符、业务标识符的信息
    /// </summary>
    string? ISmsSubResult.GetRecord()
        => $"requestId: {Response.RequestId}";

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
