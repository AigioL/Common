using AigioL.Common.Stm.Models.WebApi.Generals;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Inventories;

public sealed partial record class InventoryAssetDescriptionBody : TypeValueModel<string?, string?>
{
    public string? Name { get; set; }
}