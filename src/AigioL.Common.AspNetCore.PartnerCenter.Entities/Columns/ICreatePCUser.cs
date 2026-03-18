using AigioL.Common.AspNetCore.PartnerCenter.Entities;

namespace AigioL.Common.AspNetCore.PartnerCenter.Columns;

/// <inheritdoc cref="ICreatePCUserId.CreatePCUserId"/>
public interface ICreatePCUser
{
    /// <inheritdoc cref="ICreatePCUserId.CreatePCUserId"/>
    PCUser? CreatePCUser { get; set; }
}
