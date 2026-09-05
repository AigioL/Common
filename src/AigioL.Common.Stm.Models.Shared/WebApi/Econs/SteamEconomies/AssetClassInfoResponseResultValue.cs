using AigioL.Common.Stm.Models.Converters;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Econs.SteamEconomies;

public sealed partial record class AssetClassInfoResponseResultValue
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("classid")]
    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong ClassId { get; set; }

    [JsonPropertyName("instanceid")]
    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong InstanceId { get; set; }

    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }

    [JsonPropertyName("icon_url_large")]
    public string? IconUrlLarge { get; set; }

    [JsonPropertyName("icon_drag_url")]
    public string? IconDragUrl { get; set; }

    public string? Name { get; set; }

    [JsonPropertyName("market_hash_name")]
    public string? MarketHashName { get; set; }

    [JsonPropertyName("market_name")]
    public string? MarketName { get; set; }

    [JsonPropertyName("name_color")]
    public string? NameColor { get; set; }

    [JsonPropertyName("background_color")]
    public string? BackgroundColor { get; set; }

    public string? Type { get; set; }

    public int Tradable { get; set; }

    [JsonPropertyName("marketable")]
    public int MarkeTable { get; set; }

    public int Commodity { get; set; }

    [JsonPropertyName("market_tradable_restriction")]
    public int MarketTradableRestriction { get; set; }

    [JsonPropertyName("market_marketable_restriction")]
    public int MarketTableRestriction { get; set; }

    [JsonPropertyName("descriptions")]
    public Dictionary<int, AssetClassInfoResponseResultValueDescription> Descriptions { get; set; } = new();

    [JsonPropertyName("owner_descriptions")]
    public string? OwnerDescriptions { get; set; }

    public Dictionary<int, AssetClassInfoResponseResultValueTag> Tags { get; set; } = new();
}
