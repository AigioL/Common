using AigioL.Common.Stm.Models.Converters;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.SteamTrades;

public sealed partial record class TradeOfferBody
{
    [JsonPropertyName("newversion")]
    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool NewVersion { get; set; } = true;

    public string Version { get; set; } = "2";

    public TradeOfferBodyMeOrThem Me { get; set; } = new();

    public TradeOfferBodyMeOrThem Them { get; set; } = new();
}
