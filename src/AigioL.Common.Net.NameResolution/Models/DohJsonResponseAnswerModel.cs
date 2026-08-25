using System.Text.Json.Serialization;

namespace AigioL.Common.Net.NameResolution.Models;

internal sealed class DohJsonResponseAnswerModel
{
    ///// <summary>
    ///// 始终与“问题”部分中的名称相匹配
    ///// </summary>
    //[JsonPropertyName("name")]
    //public string? Name { get; set; }

    /// <summary>
    /// A - 标准DNS资源记录类型
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    /// 记录的生存时间（以秒为单位）
    /// </summary>
    [JsonPropertyName("TTL")]
    public long TTL { get; set; }

    /// <summary>
    /// A 的数据 - IP 地址（以文本形式）
    /// </summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }
}