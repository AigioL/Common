using AigioL.Common.Stm.Models.Converters;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Econs;

public sealed partial record class TradeOfferResponseBodyOffer
{
    [JsonPropertyName("tradeofferid")]
    [JsonConverter(typeof(NullableUInt64ToStringJsonConverter))]
    public ulong? TradeOfferId { get; set; }

    [JsonPropertyName("accountid_other")]
    public int AccountIdOther { get; set; }

    public string? Message { get; set; }

    [JsonPropertyName("expiration_time")]
    [JsonConverter(typeof(UnixTimeSecondsToDateTimeOffsetConverter))]
    public DateTimeOffset ExpirationTime { get; set; }

    [JsonPropertyName("trade_offer_state")]
    public int TradeOfferState { get; set; }

    /// <summary>
    /// 给出去的物品资产
    /// </summary>
    [JsonPropertyName("items_to_give")]
    public TradeOfferResponseBodyOfferItem[] ItemsToGive { get; set; } = [];

    /// <summary>
    /// 接受的物品资产
    /// </summary>
    [JsonPropertyName("items_to_receive")]
    public TradeOfferResponseBodyOfferItem[] ItemsToReceive { get; set; } = [];

    [JsonPropertyName("is_our_offer")]
    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool IsOurOffer { get; set; }

    [JsonPropertyName("time_created")]
    [JsonConverter(typeof(UnixTimeSecondsToDateTimeOffsetConverter))]
    public DateTimeOffset TimeCreated { get; set; }

    [JsonPropertyName("time_updated")]
    [JsonConverter(typeof(UnixTimeSecondsToDateTimeOffsetConverter))]
    public DateTimeOffset TimeUpdated { get; set; }

    [JsonPropertyName("from_real_time_trade")]
    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool FromRealTimeTrade { get; set; }

    [JsonPropertyName("escrow_end_date")]
    public long EscrowEndDate { get; set; } // 值为 0，不像时间戳

    [JsonPropertyName("confirmation_method")]
    public int ConfirmationMethod { get; set; }

    [JsonPropertyName("eresult")]
    public int EResult { get; set; }

    [JsonPropertyName("delay_settlement")]
    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool DelaySettlement { get; set; }

    [JsonPropertyName("settlement_date")]
    public long SettlementDate { get; set; } // 值三位数整数，不像时间戳
}