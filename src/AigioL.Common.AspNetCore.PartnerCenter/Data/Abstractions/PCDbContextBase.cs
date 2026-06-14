using AigioL.Common.AspNetCore.PartnerCenter.Columns;
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
using TableNames = AigioL.Common.AspNetCore.PartnerCenter.Data.Abstractions.IPCDbContextBase.TableNames;

namespace AigioL.Common.AspNetCore.PartnerCenter.Data.Abstractions;

/// <summary>
/// 合作伙伴后台的 Identity 数据库上下文基类
/// </summary>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TRole"></typeparam>
/// <typeparam name="TUserRole"></typeparam>
public abstract partial class PCDbContextBase<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUser,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TRole,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUserRole> :
    IdentityDbContext<TUser, TRole, Guid, PCUserClaim, TUserRole, PCUserLogin, PCRoleClaim, PCUserToken>, IPCDbContext2, IPCDbContextBase, IDbContextBase
    where TUser : PCUser
    where TRole : PCRole
    where TUserRole : PCUserRole
{
    /// <inheritdoc/>
    DbContext IDbContextBase.GetDbContext() => this;

    /// <inheritdoc/>
    DatabaseFacade IDbContextBase.GetDatabase() => Database;

    protected readonly IHttpContextAccessor httpContextAccessor;

    protected PCDbContextBase(
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

        // 重命名 Identity PC 相关表名
        TableNames.RenamePCTables<TUser, TRole, TUserRole>(b);

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

    /// <inheritdoc cref="PCButton"/>
    public DbSet<PCButton> Buttons { get; set; }

    /// <inheritdoc cref="PCMenu"/>
    public DbSet<PCMenu> Menus { get; set; }

    /// <inheritdoc cref="PCMenuButton"/>
    public DbSet<PCMenuButton> MenuButtons { get; set; }

    /// <inheritdoc cref="PCMenuButtonRole"/>
    public DbSet<PCMenuButtonRole> MenuButtonRoles { get; set; }

    /// <inheritdoc cref="PCTenant"/>
    public DbSet<PCTenant> Tenants { get; set; }

    sealed record PCUserIdM(Guid? UserId);

    protected virtual void OnSaveChanges()
    {
        PCUserIdM? userIdM = null;
        Guid? GetCurrentlyLoggedInUserId()
        {
            if (userIdM == null)
            {
                userIdM = new PCUserIdM(GetUserId(httpContextAccessor.HttpContext));
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
                    if (entity.Entity is IOperatorPCUserId operatorUserId) // 设置操作人
                    {
                        var uid = GetCurrentlyLoggedInUserId();
                        if (uid.HasValue)
                        {
                            operatorUserId.OperatorPCUserId = uid;
                        }
                    }
                    break;
                case EntityState.Added:
                    if (entity.Entity is ICreatePCUserId createUserId) // 设置创建人
                    {
                        var uid = GetCurrentlyLoggedInUserId();
                        if (uid.HasValue)
                        {
                            createUserId.CreatePCUserId = uid.Value;
                        }
                    }
                    else if (entity.Entity is ICreatePCUserIdNullable createUserIdNullable)
                    {
                        var uid = GetCurrentlyLoggedInUserId();
                        if (uid.HasValue)
                        {
                            createUserIdNullable.CreatePCUserId = uid.Value;
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
