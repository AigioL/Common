using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.PartnerCenter.Models.Menus;

public sealed class PCMenuButtonModel : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? IconUrl { get; set; }

    public string? Key { get; set; }

    public string? Url { get; set; }

    public long Sort { get; set; }

    public long Order => Sort;

    public List<PCMenuButtonModel> Children { get; set; } = null!;

    public List<PCButtonModel> Buttons { get; set; } = null!;

    public bool HasBtnRole(PCButtonType type) => Buttons.Any(x => x.Type == type);
}