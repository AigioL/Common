using AigioL.Common.Stm.Models.Converters;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Econ;

public partial record class TradeHistoryResponseBodyTradeAssetsReceived
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("contextid")]
    public int ContextId { get; set; }

    [JsonPropertyName("assetid")]
    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong AssetId { get; set; }

    [JsonPropertyName("classid")]
    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong ClassId { get; set; }

    [JsonPropertyName("instanceid")]
    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong InstanceId { get; set; }

    public int Amount { get; set; }

    [JsonPropertyName("new_assetid")]
    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong NewAssetId { get; set; }

    [JsonPropertyName("new_contextid")]
    public int NewContextId { get; set; }
}
