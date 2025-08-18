using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;
using TableNames = AigioL.Common.AspNetCore.AppCenter.Data.Abstractions.IAppDbContextBase.TableNames;

namespace AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;

/// <summary>
/// 客户端 App WebApi 的数据库上下文基类
/// </summary>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TRole"></typeparam>
/// <typeparam name="TUserRole"></typeparam>
public abstract partial class AppDbContextBase<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUser,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TRole,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUserRole> :
    IdentityDbContext<TUser, TRole, Guid, IdentityUserClaim<Guid>, TUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>, IAppDbContextBase
    where TUser : IdentityUser<Guid>
    where TRole : IdentityRole<Guid>
    where TUserRole : IdentityUserRole<Guid>
{
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
    }
}
