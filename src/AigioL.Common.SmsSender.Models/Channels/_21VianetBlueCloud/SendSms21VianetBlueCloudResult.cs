using AigioL.Common.SmsSender.Models.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.SmsSender.Models.Channels._21VianetBlueCloud;

/// <summary>
/// https://bcssstorage.blob.core.chinacloudapi.cn/docs/CCS/%E8%93%9D%E4%BA%91%E7%94%A8%E6%88%B7%E8%BF%9E%E6%8E%A5%E6%9C%8D%E5%8A%A1%E6%8A%80%E6%9C%AF%E6%96%87%E6%A1%A3(%E7%9F%AD%E4%BF%A1%2B%E7%99%BB%E5%BD%95).pdf
/// </summary>
public class SendSms21VianetBlueCloudResult : ISmsSubResult, IJsonSerializerContext
{
    static JsonSerializerContext IJsonSerializerContext.GetDefault() => SmsSenderJsonSerializerContext.Default;

    /// <summary>
    /// 短信发送的 ID，用于后续查询
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("messageId")]
    public string? MessageId { get; set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("sendTime")]
    public string? SendTime { get; set; }

    /// <summary>
    /// 返回包含了短信发送的 MessageId 和发送时间 SendTime 的信息
    /// </summary>
    string? ISmsSubResult.GetRecord() => $"messageId: {MessageId}, sendTime: {SendTime}";

    public virtual string GetJsonString(bool writeIndented = false)
    {
        var json = JsonSerializer.Serialize(this, SmsSenderJsonSerializerContext.Default.SendSms21VianetBlueCloudResult);
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