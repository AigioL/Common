namespace AigioL.Common.AspNetCore.AdminCenter.Models.Menus;

public sealed class ACMenuButtonModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? IconUrl { get; set; }

    public string? Key { get; set; }

    public string? Url { get; set; }

    public long Sort { get; set; }

    public long Order => Sort;

    public List<ACMenuButtonModel> Children { get; set; } = null!;

    public List<ACButtonModel> Buttons { get; set; } = null!;

    public bool HasBtnRole(ACButtonType type) => Buttons.Any(x => x.Type == type);
}