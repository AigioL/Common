using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TableNames = AigioL.Common.AspNetCore.AppCenter.Data.Abstractions.IAppDbContextBase.TableNames;

namespace AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;

/// <summary>
/// 客户端 App WebApi 的数据库上下文基类
/// </summary>
public abstract partial class AppDbContextBase :
    IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>,
    IAppDbContextBase,
    IDbContextBase,
    IIdentityDbContext
{
    /// <inheritdoc/>
    DbContext IDbContextBase.GetDbContext() => this;

    /// <inheritdoc/>
    DatabaseFacade IDbContextBase.GetDatabase() => Database;

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // 重命名 Identity 相关表名
        IAppDbContextBase.ToIdentitysTable(b);
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
