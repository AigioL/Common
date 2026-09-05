using AigioL.Common.Stm.Models.Converters;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.SteamTrades;

public sealed partial record class TradeOfferBodyAsset
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("contextid")]
    public int ContextId { get; set; }

    public int Amount { get; set; }

    [JsonPropertyName("assetid")]
    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong AssetId { get; set; }
}
