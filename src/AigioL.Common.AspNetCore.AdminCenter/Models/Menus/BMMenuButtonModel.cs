namespace AigioL.Common.AspNetCore.AdminCenter.Models.Menus;

public sealed class BMMenuButtonModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? IconUrl { get; set; }

    public string? Key { get; set; }

    public string? Url { get; set; }

    public long Sort { get; set; }

    public long Order => Sort;

    public List<BMMenuButtonModel> Children { get; set; } = null!;

    public List<BMButtonModel> Buttons { get; set; } = null!;

    public bool HasBtnRole(BMButtonType type) => Buttons.Any(x => x.Type == type);
}