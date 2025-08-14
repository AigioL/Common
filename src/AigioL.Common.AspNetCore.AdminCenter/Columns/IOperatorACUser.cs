using AigioL.Common.AspNetCore.AdminCenter.Entities;

namespace AigioL.Common.AspNetCore.AdminCenter.Columns;

/// <inheritdoc cref="ICreateUserId.CreateUserId"/>
public interface IOperatorACUser
{
    /// <inheritdoc cref="IOperatorUserId.OperatorUserId"/>
    ACUser? OperatorUser { get; set; }
}
