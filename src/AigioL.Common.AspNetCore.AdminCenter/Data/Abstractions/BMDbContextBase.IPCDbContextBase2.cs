using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using Microsoft.EntityFrameworkCore;
using IPCDbContextBase2 = AigioL.Common.Repositories.EntityFrameworkCore.Abstractions.IIdentityDbContext<
    AigioL.Common.AspNetCore.PartnerCenter.Entities.PCUser,
    AigioL.Common.AspNetCore.PartnerCenter.Entities.PCRole,
    System.Guid,
    AigioL.Common.AspNetCore.PartnerCenter.Entities.PCUserClaim,
    AigioL.Common.AspNetCore.PartnerCenter.Entities.PCUserRole,
    AigioL.Common.AspNetCore.PartnerCenter.Entities.PCUserLogin,
    AigioL.Common.AspNetCore.PartnerCenter.Entities.PCRoleClaim,
    AigioL.Common.AspNetCore.PartnerCenter.Entities.PCUserToken>;

namespace AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;

partial class BMDbContextBase<TUser, TRole, TUserRole> : IPCDbContextBase2
{
    public DbSet<PCUser> PCUsers { get; set; } = null!;

    DbSet<PCUser> IPCDbContextBase2.Users => PCUsers;

    public DbSet<PCUserClaim> PCUserClaims { get; set; } = null!;

    DbSet<PCUserClaim> IPCDbContextBase2.UserClaims => PCUserClaims;

    public DbSet<PCUserLogin> PCUserLogins { get; set; } = null!;

    DbSet<PCUserLogin> IPCDbContextBase2.UserLogins => PCUserLogins;

    public DbSet<PCUserToken> PCUserTokens { get; set; } = null!;

    DbSet<PCUserToken> IPCDbContextBase2.UserTokens => PCUserTokens;

    //public DbSet<TUserPasskey> PCUserPasskeys { get; set; } = null!;

    public DbSet<PCUserRole> PCUserRoles { get; set; } = null!;

    DbSet<PCUserRole> IPCDbContextBase2.UserRoles => PCUserRoles;

    public DbSet<PCRole> PCRoles { get; set; } = null!;

    DbSet<PCRole> IPCDbContextBase2.Roles => PCRoles;

    public DbSet<PCRoleClaim> PCRoleClaims { get; set; } = null!;

    DbSet<PCRoleClaim> IPCDbContextBase2.RoleClaims => PCRoleClaims;
}