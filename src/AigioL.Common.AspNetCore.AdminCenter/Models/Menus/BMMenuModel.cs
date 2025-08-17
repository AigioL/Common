namespace AigioL.Common.AspNetCore.AdminCenter.Models.Menus;

public sealed class BMMenuModel
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string Name { get; set; } = null!;

    public string? IconUrl { get; set; }

    public string? Key { get; set; }

    public long Sort { get; set; }

    public long Order => Sort;

    public string? Note { get; set; }

    public string? Remarks => Note;

    public string? Url { get; set; }
}
