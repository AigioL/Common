using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Econs;

public sealed partial record class TradeOffersResponseBody
{
    [JsonPropertyName("trade_offers_sent")]
    public TradeOfferResponseBodyOffer[] TradeOffersSent { get; set; } = [];

    [JsonPropertyName("next_cursor")]
    public uint NextCursor { get; set; }
}