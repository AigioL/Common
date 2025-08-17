using AigioL.Common.AspNetCore.AdminCenter.Entities;

namespace AigioL.Common.AspNetCore.AdminCenter.Columns;

/// <inheritdoc cref="ICreateUserId.CreateUserId"/>
public interface ICreateBMUser
{
    /// <inheritdoc cref="ICreateUserId.CreateUserId"/>
    BMUser? CreateUser { get; set; }
}
