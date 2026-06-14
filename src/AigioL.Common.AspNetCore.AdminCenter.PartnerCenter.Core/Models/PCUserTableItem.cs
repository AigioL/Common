using AigioL.Common.AspNetCore.PartnerCenter.Models;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Models;

public sealed record class PCUserTableItem : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    public PCUserType UserType { get; set; }

    public string? PhoneNumber { get; set; }

    public string? PhoneNumberRegionCode { get; set; }

    public Guid[] BusinessIds { get; set; } = [];

    public bool Disable { get; set; }

    public DateTimeOffset CreateTime { get; set; }

    public DateTimeOffset UpdateTime { get; set; }

    public Guid? CreateUserId { get; set; }

    public string? CreateUser { get; set; }

    public Guid? OperatorUserId { get; set; }

    public string? OperatorUser { get; set; }
}