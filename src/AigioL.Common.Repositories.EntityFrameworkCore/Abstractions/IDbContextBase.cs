using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

/// <inheritdoc cref="DbContext"/>
public interface IDbContextBase
{
    /// <inheritdoc cref="DbContext"/>
    DbContext GetDbContext();

    /// <inheritdoc cref="DbContext.Database"/>
    DatabaseFacade GetDatabase();
}
