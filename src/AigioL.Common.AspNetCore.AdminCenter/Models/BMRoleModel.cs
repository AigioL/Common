using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AdminCenter.Models;

public sealed partial class BMRoleModel : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
}
