using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;

/// <summary>
/// 管理后台的 Identity 数据库上下文基类
/// </summary>
/// <typeparam name="TACUser"></typeparam>
/// <typeparam name="TACRole"></typeparam>
public abstract class AdminCenterIdentityDbContextBase<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACUser,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACRole> :
    IdentityDbContext<TACUser, TACRole, Guid>
    where TACUser : IdentityUser<Guid>
    where TACRole : IdentityRole<Guid>
{
    protected readonly IHttpContextAccessor? a;

    protected AdminCenterIdentityDbContextBase(IServiceProvider serviceProvider, DbContextOptions options) : base(options)
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 重命名 Identity 相关表名
        builder.Entity<TACUser>().ToTable(TableNames.Users);
        builder.Entity<TACRole>().ToTable(TableNames.Roles);
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable(TableNames.RoleClaims);
        builder.Entity<IdentityUserClaim<Guid>>().ToTable(TableNames.UserClaims);
        builder.Entity<IdentityUserLogin<Guid>>().ToTable(TableNames.UserLogins);
        builder.Entity<IdentityUserRole<Guid>>().ToTable(TableNames.UserRoles);
        builder.Entity<IdentityUserToken<Guid>>().ToTable(TableNames.UserTokens);
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
}
