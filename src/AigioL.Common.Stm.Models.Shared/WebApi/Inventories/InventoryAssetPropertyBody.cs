using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Inventories;

public partial record class InventoryAssetPropertyBody
{
    [JsonPropertyName("propertyid")]
    public int PropertyId { get; set; }

    [JsonPropertyName("int_value")]
    public int? IntValue { get; set; }

    [JsonPropertyName("float_value")]
    public float? FloatValue { get; set; }

    [JsonPropertyName("string_value")]
    public string? StringValue { get; set; }
}
