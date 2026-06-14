using AigioL.Common.AspNetCore.PartnerCenter.Models;

namespace AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Models;

public sealed class PCUserOrderFilterModel
{
    public Guid[]? BusinessIds { get; set; }

    public PCUserType? UserType { get; set; }
}