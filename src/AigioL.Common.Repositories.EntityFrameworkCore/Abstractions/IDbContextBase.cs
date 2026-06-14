using Microsoft.AspNetCore.Identity;
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

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IIdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>
    where TUserRole : IdentityUserRole<TKey>
    where TUserLogin : IdentityUserLogin<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>
    where TUserToken : IdentityUserToken<TKey>
{
    DbSet<TUser> Users { get; }

    DbSet<TUserClaim> UserClaims { get; }

    DbSet<TUserLogin> UserLogins { get; }

    DbSet<TUserToken> UserTokens { get; }

    DbSet<TUserRole> UserRoles { get; }

    DbSet<TRole> Roles { get; }

    DbSet<TRoleClaim> RoleClaims { get; }
}