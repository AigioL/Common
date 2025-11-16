using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.EntityFrameworkCore.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // 与 BMDbContextBase 同步调用 BuildEntities 扩展函数
        b.BuildEntities(AppendBuildEntities_);
    }

    Action<EntityTypeBuilder>? AppendBuildEntities_(ModelBuilder modelBuilder, IMutableEntityType entityType, Type type, Action<EntityTypeBuilder>? buildAction)
    {
        buildAction = AppendBuildEntities(modelBuilder, entityType, type, buildAction);
        return buildAction;
    }

    /// <summary>
    /// 用于追加构建实体的操作方法
    /// </summary>
    protected virtual Action<EntityTypeBuilder>? AppendBuildEntities(ModelBuilder modelBuilder, IMutableEntityType entityType, Type type, Action<EntityTypeBuilder>? buildAction)
    {
        return buildAction;
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
