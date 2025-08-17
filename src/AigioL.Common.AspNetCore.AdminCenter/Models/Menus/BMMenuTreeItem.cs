namespace AigioL.Common.AspNetCore.AdminCenter.Models.Menus;

public sealed class BMMenuTreeItem
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string Name { get; set; } = null!;

    public string? Key { get; set; }

    public string? Url { get; set; }

    public long Sort { get; set; }

    public List<BMMenuTreeItem> Children { get; set; } = null!;
}
