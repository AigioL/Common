using AigioL.Common.Stm.Models.Converters;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Inventories;

public sealed partial record class InventoryAssetProperty
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("contextid")]
    public int ContextId { get; set; }

    [JsonPropertyName("assetid")]
    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong AssetId { get; set; }

    [JsonPropertyName("asset_properties")]
    public InventoryAssetPropertyBody[]? AssetProperties { get; set; }
}