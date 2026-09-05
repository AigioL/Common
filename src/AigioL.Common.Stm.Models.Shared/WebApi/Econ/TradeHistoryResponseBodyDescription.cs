using AigioL.Common.Stm.Models.Converters;
using AigioL.Common.Stm.Models.WebApi.General;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Econ;

public partial record class TradeHistoryResponseBodyDescription
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("classid")]
    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong ClassId { get; set; }

    [JsonPropertyName("instanceid")]
    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong InstanceId { get; set; }

    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool Currency { get; set; }

    [JsonPropertyName("background_color")]
    public string? BackgroundColor { get; set; }

    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }

    [JsonPropertyName("icon_url_large")]
    public string? IconUrlLarge { get; set; }

    public TypeValueModel[]? Descriptions { get; set; }

    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool Tradable { get; set; }

    public LinkNameModel[]? Actions { get; set; }

    [JsonPropertyName("owner_actions")]
    public LinkNameModel[]? OwnerActions { get; set; }

    public string? Name { get; set; }

    public string? Type { get; set; }

    [JsonPropertyName("market_name")]
    public string? MarketName { get; set; }

    [JsonPropertyName("market_hash_name")]
    public string? MarketHashName { get; set; }

    [JsonPropertyName("market_fee_app")]
    public int MarketFeeApp { get; set; }

    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool Commodity { get; set; }

    [JsonPropertyName("market_tradable_restriction")]
    public int MarketTradableRestriction { get; set; }

    [JsonPropertyName("market_marketable_restriction")]
    public int MarketTableRestriction { get; set; }

    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool MarkeTable { get; set; }

    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool Sealed { get; set; }
}