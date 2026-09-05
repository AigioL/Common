using AigioL.Common.Stm.Models.Converters;
using AigioL.Common.Stm.Models.WebApi.Generals;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Inventories;

public sealed partial record class InventoryAssetDescription
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("classid")]
    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong ClassId { get; set; }

    [JsonPropertyName("instanceid")]
    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong InstanceId { get; set; }

    public int Currency { get; set; }

    [JsonPropertyName("background_color")]
    public string? BackgroundColor { get; set; }

    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }

    public InventoryAssetDescriptionBody[]? Descriptions { get; set; }

    public int Tradable { get; set; }

    public LinkNameModel[]? Actions { get; set; }

    public string? Name { get; set; }

    [JsonPropertyName("name_color")]
    public string? NameColor { get; set; }

    public string? Type { get; set; }

    [JsonPropertyName("market_name")]
    public string? MarketName { get; set; }

    [JsonPropertyName("market_hash_name")]
    public string? MarketHashName { get; set; }

    [JsonPropertyName("market_actions")]
    public LinkNameModel[]? MarketActions { get; set; }

    public int Commodity { get; set; }

    [JsonPropertyName("market_tradable_restriction")]
    public int MarketTradableRestriction { get; set; }

    [JsonPropertyName("market_marketable_restriction")]
    public int MarketTableRestriction { get; set; }

    public int MarkeTable { get; set; }

    public InventoryAssetDescriptionTag[]? Tags { get; set; }

    public int Sealed { get; set; }
}