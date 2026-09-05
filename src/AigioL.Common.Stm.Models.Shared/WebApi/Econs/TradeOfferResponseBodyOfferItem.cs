using AigioL.Common.Stm.Models.Converters;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Econs;

public sealed partial record class TradeOfferResponseBodyOfferItem
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

    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool Missing { get; set; }

    [JsonPropertyName("est_usd")]
    public decimal EstUsd { get; set; }
}