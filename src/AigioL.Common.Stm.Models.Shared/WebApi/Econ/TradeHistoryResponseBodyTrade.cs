using AigioL.Common.Stm.Models.Converters;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Econ;

public partial record class TradeHistoryResponseBodyTrade
{
    [JsonConverter(typeof(NullableUInt64ToStringJsonConverter))]
    public ulong? TradeId { get; set; }

    [JsonPropertyName("steamid_other")]
    [JsonConverter(typeof(NullableUInt64ToStringJsonConverter))]
    public ulong? SteamIdOther { get; set; }

    [JsonPropertyName("time_init")]
    [JsonConverter(typeof(UnixTimeSecondsToDateTimeOffsetConverter))]
    public DateTimeOffset TimeInit { get; set; }

    public int Status { get; set; }

    [JsonPropertyName("assets_received")]
    public TradeHistoryResponseBodyTradeAssetsReceived[] AssetsReceived { get; set; } = [];
}