using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.PartnerCenter.Models.Menus;

public sealed class PCButtonModel : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public PCButtonType Type { get; set; }

    public bool Disable { get; set; }
}