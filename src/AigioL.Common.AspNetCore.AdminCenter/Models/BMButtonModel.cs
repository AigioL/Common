using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AdminCenter.Models.Menus;

public sealed class BMButtonModel : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public BMButtonType Type { get; set; }

    public bool Disable { get; set; }
}