namespace AigioL.Common.AspNetCore.AdminCenter.Models.Menus;

public sealed class ACButtonModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public ACButtonType Type { get; set; }

    public bool Disable { get; set; }
}