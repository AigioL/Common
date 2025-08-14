using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.EntityFrameworkCore.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;

/// <summary>
/// 管理后台的 Identity 数据库上下文基类
/// </summary>
/// <typeparam name="TACUser"></typeparam>
/// <typeparam name="TACRole"></typeparam>
/// <typeparam name="TACUserRole"></typeparam>
public abstract partial class ACIdentityDbContextBase<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACUser,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACRole,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACUserRole> :
    IdentityDbContext<TACUser, TACRole, Guid, IdentityUserClaim<Guid>, TACUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    where TACUser : ACUser
    where TACRole : ACRole
    where TACUserRole : ACUserRole
{
    protected readonly IHttpContextAccessor? a;

    protected ACIdentityDbContextBase(IServiceProvider serviceProvider, DbContextOptions options) : base(options)
    {
        a = serviceProvider.GetService<IHttpContextAccessor>();
    }

    /// <summary>
    /// 从当前 Http 上下文中获取管理后台用户 Id
    /// </summary>
    /// <returns></returns>
    protected virtual Guid? GetCurrentUserId()
    {
        var ctx = a?.HttpContext;
        if (ctx != null)
        {
            var userManager = ctx.RequestServices.GetRequiredService<UserManager<TACUser>>();
            var userId = userManager.GetUserId(ctx.User);
            if (ShortGuid.TryParse(userId, out Guid userIdG))
            {
                return userIdG;
            }
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
        b.Entity<TACUser>().ToTable(TableNames.Users);
        b.Entity<TACRole>().ToTable(TableNames.Roles);
        b.Entity<IdentityRoleClaim<Guid>>().ToTable(TableNames.RoleClaims);
        b.Entity<IdentityUserClaim<Guid>>().ToTable(TableNames.UserClaims);
        b.Entity<IdentityUserLogin<Guid>>().ToTable(TableNames.UserLogins);
        b.Entity<TACUserRole>().ToTable(TableNames.UserRoles);
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

    public static class TableNames
    {
        public const string Users = "ACUsers";
        public const string Roles = "ACRoles";
        public const string RoleClaims = "ACRoleClaims";
        public const string UserClaims = "ACUserClaims";
        public const string UserLogins = "ACUserLogins";
        public const string UserRoles = "ACUserRoles";
        public const string UserTokens = "ACUserTokens";
    }

    /// <inheritdoc cref="ACButton"/>
    public DbSet<ACButton> Buttons { get; set; }

    /// <inheritdoc cref="ACInformational"/>
    public DbSet<ACInformational> Informationals { get; set; }

    /// <inheritdoc cref="ACMenu"/>
    public DbSet<ACMenu> Menus { get; set; }

    /// <inheritdoc cref="ACMenuButton"/>
    public DbSet<ACMenuButton> MenuButtons { get; set; }

    /// <inheritdoc cref="ACMenuButtonRole"/>
    public DbSet<ACMenuButtonRole> MenuButtonRoles { get; set; }

    /// <inheritdoc cref="ACTenant"/>
    public DbSet<ACTenant> Tenants { get; set; }
}
