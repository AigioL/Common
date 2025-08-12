using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.SmsSender.Models.Channels._21VianetBlueCloud;

/// <summary>
/// 请求数据
/// </summary>
public sealed class _21VianetBlueRequestData : IJsonSerializerContext
{
    static JsonSerializerContext IJsonSerializerContext.GetDefault() => SmsSenderJsonSerializerContext.Default;

    /// <summary>
    /// 接收手机号
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("phoneNumber")]
    public string[]? PhoneNumber { get; set; }

    /// <summary>
    /// 下发扩展码，两位纯数字
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("extend")]
    public string? ExtendCode { get; set; }

    /// <summary>
    /// 消息正文
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("messageBody")]
    public _21VianetBlueMessageBody? MessageBody { get; set; }

    public string GetJsonString(bool writeIndented = false)
    {
        var json = JsonSerializer.Serialize(this, SmsSenderJsonSerializerContext.Default._21VianetBlueRequestData);
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
/// 消息正文
/// </summary>
public sealed class _21VianetBlueMessageBody : IJsonSerializerContext
{
    static JsonSerializerContext IJsonSerializerContext.GetDefault() => SmsSenderJsonSerializerContext.Default;

    /// <summary>
    /// 短信模板名称
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("templateName")]
    public string? TemplateName { get; set; }

    /// <summary>
    /// 短信模板参数，和模板中变量一一对应,没有变量则不需要
    /// </summary>
    [global::System.Text.Json.Serialization.JsonPropertyName("templateParam")]
    public Dictionary<string, string> TemplateParam { get; set; } = new();

    public string GetJsonString(bool writeIndented = false)
    {
        var json = JsonSerializer.Serialize(this, SmsSenderJsonSerializerContext.Default._21VianetBlueMessageBody);
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