using AigioL.Common.Stm.Models.WebApi.Generals;

namespace AigioL.Common.Stm.Models.WebApi.Econs.SteamEconomies;

public sealed partial record class AssetClassInfoResponseResultValueDescription : TypeValueModel<string?, string?>
{
    public string? Name { get; set; }
}
