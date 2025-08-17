using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AdminCenter.Models.Menus;

public sealed class BMMenuEdit
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string Name { get; set; } = null!;

    public string? IconUrl { get; set; }

    public string Key { get; set; } = null!;

    [NotMapped]
    public long Order { get => Sort; set => Sort = value; }

    public long Sort { get; set; }

    [NotMapped]
    public string? Remarks { get => Note; set => Note = value; }

    public string? Note { get; set; }

    public string Url { get; set; } = null!;

    public bool SoftDeleted { get; set; }
}
