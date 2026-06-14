using AigioL.Common.AspNetCore.AdminCenter.Columns;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;
using PC_TableNames = AigioL.Common.AspNetCore.PartnerCenter.Data.Abstractions.IPCDbContextBase.TableNames;
using TableNames = AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions.IBMDbContextBase.TableNames;

namespace AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;

/// <summary>
/// 管理后台的 Identity 数据库上下文基类
/// </summary>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TRole"></typeparam>
/// <typeparam name="TUserRole"></typeparam>
public abstract partial class BMDbContextBase<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUser,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TRole,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUserRole> :
    IdentityDbContext<TUser, TRole, Guid, BMUserClaim, TUserRole, BMUserLogin, BMRoleClaim, BMUserToken>, IBMDbContextBase, IDbContextBase
    where TUser : BMUser
    where TRole : BMRole
    where TUserRole : BMUserRole
{
    /// <inheritdoc/>
    DbContext IDbContextBase.GetDbContext() => this;

    /// <inheritdoc/>
    DatabaseFacade IDbContextBase.GetDatabase() => Database;

    protected readonly IHttpContextAccessor httpContextAccessor;

    protected BMDbContextBase(
        DbContextOptions options,
        IHttpContextAccessor httpContextAccessor) : base(options)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public virtual Guid? GetUserId(HttpContext? ctx)
    {
        if (ctx != null)
        {
            var userManager = ctx.RequestServices.GetRequiredService<UserManager<TUser>>();
            var userId = userManager.GetUserId(ctx.User);
            if (ShortGuid.TryParse(userId, out Guid userIdG))
            {
                return userIdG;
            }
        }
        return null;
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // 重命名 Identity BM 相关表名
        b.Entity<TUser>().ToTable(TableNames.Users);
        b.Entity<TRole>().ToTable(TableNames.Roles);
        b.Entity<BMRoleClaim>().ToTable(TableNames.RoleClaims);
        b.Entity<BMUserClaim>().ToTable(TableNames.UserClaims);
        b.Entity<BMUserLogin>().ToTable(TableNames.UserLogins);
        b.Entity<TUserRole>().ToTable(TableNames.UserRoles);
        b.Entity<BMUserToken>().ToTable(TableNames.UserTokens);

        // 重命名 Identity PC 相关表名
        PC_TableNames.RenamePCTables<PCUser, PCRole, PCUserRole>(b);

        // 与 AppDbContextBase 同步调用 BuildEntities 扩展函数
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

    /// <inheritdoc cref="BMButton"/>
    public DbSet<BMButton> Buttons { get; set; }

    /// <inheritdoc cref="BMMenu"/>
    public DbSet<BMMenu> Menus { get; set; }

    /// <inheritdoc cref="BMMenuButton"/>
    public DbSet<BMMenuButton> MenuButtons { get; set; }

    /// <inheritdoc cref="BMMenuButtonRole"/>
    public DbSet<BMMenuButtonRole> MenuButtonRoles { get; set; }

    /// <inheritdoc cref="BMTenant"/>
    public DbSet<BMTenant> Tenants { get; set; }

    sealed record UserIdM(Guid? UserId);

    protected virtual void OnSaveChanges()
    {
        UserIdM? userIdM = null;
        Guid? GetCurrentlyLoggedInUserId()
        {
            if (userIdM == null)
            {
                userIdM = new UserIdM(GetUserId(httpContextAccessor.HttpContext));
            }
            return userIdM?.UserId;
        }

        foreach (var entity in ChangeTracker.Entries())
        {
            switch (entity.State)
            {
                case EntityState.Modified:
                    if (entity.Entity is IUpdateTime u) // 设置更新时间
                    {
                        u.UpdateTime = DateTimeOffset.Now;
                    }
                    if (entity.Entity is IOperatorUserId operatorUserId) // 设置操作人
                    {
                        var uid = GetCurrentlyLoggedInUserId();
                        if (uid.HasValue)
                        {
                            operatorUserId.OperatorUserId = uid;
                        }
                    }
                    break;
                case EntityState.Added:
                    if (entity.Entity is ICreateUserId createUserId) // 设置创建人
                    {
                        var uid = GetCurrentlyLoggedInUserId();
                        if (uid.HasValue)
                        {
                            createUserId.CreateUserId = uid.Value;
                        }
                    }
                    else if (entity.Entity is ICreateUserIdNullable createUserIdNullable)
                    {
                        var uid = GetCurrentlyLoggedInUserId();
                        if (uid.HasValue)
                        {
                            createUserIdNullable.CreateUserId = uid.Value;
                        }
                    }
                    break;
            }
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        OnSaveChanges();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        OnSaveChanges();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
