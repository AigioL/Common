using System.Text.Json.Serialization;

namespace AigioL.Common.SmsSender.Models.Channels.TencentCloud;

public class TencentCloudResultResponse
{
    /// <summary>
    /// 唯一请求 ID，由服务端生成，每次请求都会返回（若请求因其他原因未能抵达服务端，则该次请求不会获得 RequestId）。定位问题时需要提供该次请求的 RequestId。
    /// </summary>
    [JsonPropertyName("RequestId")]
    public string RequestId { get; set; } = "";

    [JsonPropertyName("Error")]
    public TencentCloudResultResponseError? Error { get; set; }

    public virtual bool IsOk()
    {
        return Error == null;
    }
}


public class TencentCloudResultResponseError
{
    [JsonPropertyName("Code")]
    public string? Code { get; set; }

    [JsonPropertyName("Message")]
    public string? Message { get; set; }
}