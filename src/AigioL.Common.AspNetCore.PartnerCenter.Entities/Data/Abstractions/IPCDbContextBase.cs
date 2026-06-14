using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.PartnerCenter.Data.Abstractions;

public partial interface IPCDbContextBase : IDbContextBase
{
    /// <summary>
    /// 从 Http 上下文中获取管理后台用户 Id
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    Guid? GetUserId(HttpContext? ctx);

    ///// <summary>
    ///// 从当前 Http 上下文中获取管理后台用户 Id
    ///// </summary>
    ///// <returns></returns>
    //Guid? GetCurrentUserId();

    static class TableNames
    {
        public const string Prefix = "PC";
        public const string Users = Prefix + "Users";
        public const string Roles = Prefix + "Roles";
        public const string RoleClaims = Prefix + "RoleClaims";
        public const string UserClaims = Prefix + "UserClaims";
        public const string UserLogins = Prefix + "UserLogins";
        public const string UserRoles = Prefix + "UserRoles";
        public const string UserTokens = Prefix + "UserTokens";

        public static void RenamePCTables<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUser,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TRole,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUserRole>(ModelBuilder b)
            where TUser : PCUser
            where TRole : PCRole
            where TUserRole : PCUserRole
        {
            // 重命名 Identity PC 相关表名
            b.Entity<TUser>().ToTable(Users);
            b.Entity<TRole>().ToTable(Roles);
            b.Entity<PCRoleClaim>().ToTable(RoleClaims);
            b.Entity<PCUserClaim>().ToTable(UserClaims);
            b.Entity<PCUserLogin>().ToTable(UserLogins);
            b.Entity<TUserRole>().ToTable(UserRoles);
            b.Entity<PCUserToken>().ToTable(UserTokens);
        }
    }

    public static void OnModelCreatingVersion2(IInfrastructure<IServiceProvider> accessor, ModelBuilder builder)
        => IAppDbContextBase.OnModelCreatingVersion2<PCUser, PCRole, Guid, PCUserClaim, PCUserRole, PCUserLogin, PCRoleClaim, PCUserToken>(TableNames.Prefix, accessor, builder);
}

public interface IPCDbContext2 : IDbContextBase
{
    /// <inheritdoc cref="PCButton"/>
    DbSet<PCButton> Buttons { get; }

    /// <inheritdoc cref="PCMenu"/>
    DbSet<PCMenu> Menus { get; }

    /// <inheritdoc cref="PCMenuButton"/>
    DbSet<PCMenuButton> MenuButtons { get; }

    /// <inheritdoc cref="PCMenuButtonRole"/>
    DbSet<PCMenuButtonRole> MenuButtonRoles { get; }

    /// <inheritdoc cref="PCTenant"/>
    DbSet<PCTenant> Tenants { get; }
}