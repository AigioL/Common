using AigioL.Common.AspNetCore.PartnerCenter.Models;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Models;

public sealed record class AddOrEditPCUserModel : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    public PCUserType UserType { get; set; }

    public string? PhoneNumber { get; set; }

    public string? PhoneNumberRegionCode { get; set; }

    public Guid[] BusinessIds { get; set; } = [];

    public bool Disable { get; set; }

    public string? UserName { get; set; }
}
