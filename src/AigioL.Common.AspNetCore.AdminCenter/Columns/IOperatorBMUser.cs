using AigioL.Common.AspNetCore.AdminCenter.Entities;

namespace AigioL.Common.AspNetCore.AdminCenter.Columns;

/// <inheritdoc cref="ICreateUserId.CreateUserId"/>
public interface IOperatorBMUser
{
    /// <inheritdoc cref="IOperatorUserId.OperatorUserId"/>
    BMUser? OperatorUser { get; set; }
}
