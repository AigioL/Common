using AigioL.Common.AspNetCore.PartnerCenter.Entities;

namespace AigioL.Common.AspNetCore.PartnerCenter.Columns;

/// <inheritdoc cref="IOperatorPCUserId.OperatorPCUserId"/>
public interface IOperatorPCUser
{
    /// <inheritdoc cref="IOperatorPCUserId.OperatorPCUserId"/>
    PCUser? OperatorPCUser { get; set; }
}
