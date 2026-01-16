namespace AigioL.Common.AspNetCore.AdminCenter.Models.Menus;

public partial record class BMMenuInitModel
{
    public required string Url { get; set; }

    public required string Name { get; set; }

    public required string Key { get; set; }

    public string? IconUrl { get; set; }

    public long Sort { get; set; }

    public string? Note { get; set; }
}
