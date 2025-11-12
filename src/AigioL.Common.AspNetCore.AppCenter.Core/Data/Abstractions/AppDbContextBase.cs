using AigioL.Common.AspNetCore.AppCenter.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;

/// <summary>
/// 客户端 App WebApi 的数据库上下文基类
/// </summary>
public abstract partial class AppDbContextBase :
    IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
{
    protected AppDbContextBase(DbContextOptions options) : base(options)
    {
    }

    #region 用户模块

    public DbSet<UserDelete> UserDeletes { get; set; } = null!;

    public DbSet<UserDevice> UserDevices { get; set; } = null!;

    public DbSet<UserWallet> UserWallets { get; set; } = null!;

    public DbSet<UserWalletChangeRecord> UserWalletChangeRecords { get; set; } = null!;

    public DbSet<ExternalAccount> ExternalAccounts { get; set; } = null!;

    public DbSet<UserDeleteExternalAccount> UserDeleteExternalAccounts { get; set; } = null!;

    public DbSet<UserMembership> UserMemberships { get; set; } = null!;

    public DbSet<UserMembershipChangeRecord> UserMembershipChangeRecords { get; set; } = null!;

    #endregion

    #region JsonWebToken

    public DbSet<UserJsonWebToken> UserJsonWebTokens { get; set; } = null!;

    public DbSet<UserRefreshJsonWebToken> UserRefreshJsonWebTokens { get; set; } = null!;

    #endregion
}
