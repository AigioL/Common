using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;
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

    protected BMDbContextBase(DbContextOptions options) : base(options)
    {
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

    ///// <inheritdoc/>
    //public virtual Guid? GetCurrentUserId()
    //{
    //    var ctx = httpContextAccessor?.HttpContext;
    //    if (ctx != null)
    //    {
    //        var userId = GetUserId(ctx);
    //        return userId;
    //    }
    //    return null;
    //}

    ///// <summary>
    ///// 是否允许写入空的管理后台用户 Id，当 <see cref="GetCurrentUserId"/> 返回 <see langword="null"/> 时允许创建或修改表，默认值：不允许
    ///// </summary>
    //protected virtual bool AllowEmptyCurrentUserId { get; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // 重命名 Identity 相关表名
        b.Entity<TUser>().ToTable(TableNames.Users);
        b.Entity<TRole>().ToTable(TableNames.Roles);
        b.Entity<BMRoleClaim>().ToTable(TableNames.RoleClaims);
        b.Entity<BMUserClaim>().ToTable(TableNames.UserClaims);
        b.Entity<BMUserLogin>().ToTable(TableNames.UserLogins);
        b.Entity<TUserRole>().ToTable(TableNames.UserRoles);
        b.Entity<BMUserToken>().ToTable(TableNames.UserTokens);

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

    /// <inheritdoc cref="BMInformational"/>
    public DbSet<BMInformational> Informationals { get; set; }

    /// <inheritdoc cref="BMMenu"/>
    public DbSet<BMMenu> Menus { get; set; }

    /// <inheritdoc cref="BMMenuButton"/>
    public DbSet<BMMenuButton> MenuButtons { get; set; }

    /// <inheritdoc cref="BMMenuButtonRole"/>
    public DbSet<BMMenuButtonRole> MenuButtonRoles { get; set; }

    /// <inheritdoc cref="BMTenant"/>
    public DbSet<BMTenant> Tenants { get; set; }
}
