using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.EntityFrameworkCore.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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
    IdentityDbContext<TUser, TRole, Guid, IdentityUserClaim<Guid>, TUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    where TUser : BMUser
    where TRole : BMRole
    where TUserRole : BMUserRole
{
    protected readonly IHttpContextAccessor? a;

    protected BMDbContextBase(IServiceProvider serviceProvider, DbContextOptions options) : base(options)
    {
        a = serviceProvider.GetService<IHttpContextAccessor>();
    }

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

    /// <summary>
    /// 从当前 Http 上下文中获取管理后台用户 Id
    /// </summary>
    /// <returns></returns>
    public virtual Guid? GetCurrentUserId()
    {
        var ctx = a?.HttpContext;
        if (ctx != null)
        {
            var userId = GetUserId(ctx);
            return userId;
        }
        return null;
    }

    /// <summary>
    /// 是否允许写入空的管理后台用户 Id，当 <see cref="GetCurrentUserId"/> 返回 <see langword="null"/> 时允许创建或修改表，默认值：不允许
    /// </summary>
    protected virtual bool AllowEmptyCurrentUserId { get; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // 重命名 Identity 相关表名
        b.Entity<TUser>().ToTable(TableNames.Users);
        b.Entity<TRole>().ToTable(TableNames.Roles);
        b.Entity<IdentityRoleClaim<Guid>>().ToTable(TableNames.RoleClaims);
        b.Entity<IdentityUserClaim<Guid>>().ToTable(TableNames.UserClaims);
        b.Entity<IdentityUserLogin<Guid>>().ToTable(TableNames.UserLogins);
        b.Entity<TUserRole>().ToTable(TableNames.UserRoles);
        b.Entity<IdentityUserToken<Guid>>().ToTable(TableNames.UserTokens);

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
