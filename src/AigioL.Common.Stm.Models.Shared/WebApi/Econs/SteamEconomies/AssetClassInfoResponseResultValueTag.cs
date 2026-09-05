using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Econs.SteamEconomies;

public sealed partial record class AssetClassInfoResponseResultValueTag
{
    [JsonPropertyName("internal_name")]
    public string? InternalName { get; set; }

    public string? Name { get; set; }

    public string? Category { get; set; }

    [JsonPropertyName("category_name")]
    public string? CategoryName { get; set; }
}