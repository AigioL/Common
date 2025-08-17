namespace AigioL.Common.AspNetCore.AdminCenter.Models.Menus;

public sealed class BMButtonModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public BMButtonType Type { get; set; }

    public bool Disable { get; set; }
}