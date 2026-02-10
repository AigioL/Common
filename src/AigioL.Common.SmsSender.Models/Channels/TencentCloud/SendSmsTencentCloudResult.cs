using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.SmsSender.Models.Channels.TencentCloud;

public class SendSmsTencentCloudResult : TencentCloudResultResponse, IJsonSerializerContext
{
    static JsonSerializerContext IJsonSerializerContext.GetDefault() => SmsSenderJsonSerializerContext.Default;

    /// <summary>
    /// 短信发送状态。
    /// </summary>
    [JsonPropertyName("SendStatusSet")]
    public SendStatus[]? SendStatusSet { get; set; }

    public override bool IsOk()
    {
        return base.IsOk() && (SendStatusSet?.All(x => x.Code == "Ok") ?? false);
    }

    public virtual string GetJsonString(bool writeIndented = false)
    {
        var json = JsonSerializer.Serialize(this, SmsSenderJsonSerializerContext.Default.SendSmsTencentCloudResult);
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

public class SendStatus
{
    /// <summary>
    /// 发送流水号。
    /// </summary>
    [JsonPropertyName("SerialNo")]
    public string SerialNo { get; set; } = "";

    /// <summary>
    /// 手机号码，E.164标准，+[国家或地区码][手机号] ，示例如：+8618501234444， 其中前面有一个+号 ，86为国家码，18501234444为手机号。
    /// </summary>
    [JsonPropertyName("PhoneNumber")]
    public string PhoneNumber { get; set; } = "";

    /// <summary>
    /// 计费条数，计费规则请查询 [计费策略](https://cloud.tencent.com/document/product/382/36135)。
    /// </summary>
    [JsonPropertyName("Fee")]
    public ulong? Fee { get; set; }

    /// <summary>
    /// 用户 session 内容。
    /// </summary>
    [JsonPropertyName("SessionContext")]
    public string SessionContext { get; set; } = "";

    /// <summary>
    /// 短信请求错误码，具体含义请参考 [错误码](https://cloud.tencent.com/document/api/382/55981#6.-.E9.94.99.E8.AF.AF.E7.A0.81)，发送成功返回 "Ok"。
    /// </summary>
    [JsonPropertyName("Code")]
    public string Code { get; set; } = "";

    /// <summary>
    /// 短信请求错误码描述。
    /// </summary>
    [JsonPropertyName("Message")]
    public string Message { get; set; } = "";

    /// <summary>
    /// 国家码或地区码，例如 CN、US 等，对于未识别出国家码或者地区码，默认返回 DEF，具体支持列表请参考 [国际/港澳台短信价格总览](https://cloud.tencent.com/document/product/382/18051)。
    /// </summary>
    [JsonPropertyName("IsoCode")]
    public string IsoCode { get; set; } = "";
}

