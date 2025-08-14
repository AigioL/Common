using AigioL.Common.AspNetCore.AdminCenter.Entities;

namespace AigioL.Common.AspNetCore.AdminCenter.Columns;

/// <inheritdoc cref="ICreateUserId.CreateUserId"/>
public interface ICreateACUser
{
    /// <inheritdoc cref="ICreateUserId.CreateUserId"/>
    ACUser? CreateUser { get; set; }
}
