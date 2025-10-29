using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;

public interface IAppDbContextBase : IDbContextBase
{
    static class TableNames
    {
        public const string Users = "ACUsers";
        public const string Roles = "ACRoles";
        public const string RoleClaims = "ACRoleClaims";
        public const string UserClaims = "ACUserClaims";
        public const string UserLogins = "ACUserLogins";
        public const string UserRoles = "ACUserRoles";
        public const string UserTokens = "ACUserTokens";

        public const string UserDeletes = "ACUserDeletes";
        public const string UserDeleteExternalAccounts = "ACUserDeleteExternalAccounts";
        public const string UserMemberships = "ACUserMemberships";
        public const string UserMembershipChangeRecords = "ACUserMembershipChangeRecords";
        public const string UserJsonWebTokens = "ACUserJsonWebTokens";
        public const string UserRefreshJsonWebTokens = "ACUserRefreshJsonWebTokens";
        public const string UserWallets = "ACUserWallets";
        public const string UserWalletChangeRecords = "ACUserWalletChangeRecords";
        public const string ExternalAccounts = "ACExternalAccounts";
    }

    protected static void ToIdentitysTable(ModelBuilder b)
    {
        // 重命名 Identity 相关表名
        b.Entity<User>().ToTable(TableNames.Users);
        b.Entity<Role>().ToTable(TableNames.Roles);
        b.Entity<RoleClaim>().ToTable(TableNames.RoleClaims);
        b.Entity<UserClaim>().ToTable(TableNames.UserClaims);
        b.Entity<UserLogin>().ToTable(TableNames.UserLogins);
        b.Entity<UserRole>().ToTable(TableNames.UserRoles);
        b.Entity<UserToken>().ToTable(TableNames.UserTokens);
    }

    DbSet<User> Users { get; }

    DbSet<UserClaim> UserClaims { get; }

    DbSet<UserLogin> UserLogins { get; }

    DbSet<UserToken> UserTokens { get; }

    DbSet<UserRole> UserRoles { get; }

    DbSet<Role> Roles { get; }

    DbSet<RoleClaim> RoleClaims { get; }

    protected static StoreOptions? GetStoreOptions(IInfrastructure<IServiceProvider> accessor)
        => accessor.GetService<IDbContextOptions>()
                .Extensions.OfType<CoreOptionsExtension>()
                .FirstOrDefault()?.ApplicationServiceProvider
                ?.GetService<IOptions<IdentityOptions>>()
                ?.Value?.Stores;

    /// <summary>
    /// Configures the schema needed for the identity framework for schema version 2.0
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="accessor"></param>
    /// <param name="builder">
    /// The builder being used to construct the model for this context.
    /// </param>
    private static void OnModelCreatingVersion2<
        [DynamicallyAccessedMembers(IEntity.DAMT)] TUser,
        [DynamicallyAccessedMembers(IEntity.DAMT)] TKey,
        [DynamicallyAccessedMembers(IEntity.DAMT)] TUserClaim,
        [DynamicallyAccessedMembers(IEntity.DAMT)] TUserLogin,
        [DynamicallyAccessedMembers(IEntity.DAMT)] TUserToken>(
        string prefix,
        IInfrastructure<IServiceProvider> accessor, ModelBuilder builder)
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
        where TUserLogin : IdentityUserLogin<TKey>
        where TUserToken : IdentityUserToken<TKey>
    {
        // Differences from Version 1:
        // - maxKeyLength defaults to 128
        // - PhoneNumber has a 256 max length

        var storeOptions = GetStoreOptions(accessor);
        var maxKeyLength = storeOptions?.MaxLengthForKeys ?? 0;
        if (maxKeyLength == 0)
        {
            maxKeyLength = 128;
        }
        var encryptPersonalData = storeOptions?.ProtectPersonalData ?? false;
        PersonalDataConverter? converter = null;

        builder.Entity<TUser>(b =>
        {
            b.HasKey(u => u.Id);
            b.HasIndex(u => u.NormalizedUserName).HasDatabaseName(prefix + "UserNameIndex").IsUnique();
            b.HasIndex(u => u.NormalizedEmail).HasDatabaseName(prefix + "EmailIndex");
            //b.ToTable("AspNetUsers");
            b.Property(u => u.ConcurrencyStamp).IsConcurrencyToken();

            b.Property(u => u.UserName).HasMaxLength(256);
            b.Property(u => u.NormalizedUserName).HasMaxLength(256);
            b.Property(u => u.Email).HasMaxLength(256);
            b.Property(u => u.NormalizedEmail).HasMaxLength(256);
            b.Property(u => u.PhoneNumber).HasMaxLength(256);

            if (encryptPersonalData)
            {
                converter = new PersonalDataConverter(accessor.GetService<IPersonalDataProtector>());
                var personalDataProps = typeof(TUser).GetProperties().Where(
                                prop => Attribute.IsDefined(prop, typeof(ProtectedPersonalDataAttribute)));
                foreach (var p in personalDataProps)
                {
                    if (p.PropertyType != typeof(string))
                    {
                        throw new InvalidOperationException(CanOnlyProtectStrings);
                    }
                    b.Property(typeof(string), p.Name).HasConversion(converter);
                }
            }

            b.HasMany<TUserClaim>().WithOne().HasForeignKey(uc => uc.UserId).IsRequired();
            b.HasMany<TUserLogin>().WithOne().HasForeignKey(ul => ul.UserId).IsRequired();
            b.HasMany<TUserToken>().WithOne().HasForeignKey(ut => ut.UserId).IsRequired();
        });

        builder.Entity<TUserClaim>(b =>
        {
            b.HasKey(uc => uc.Id);
            //b.ToTable("AspNetUserClaims");
        });

        builder.Entity<TUserLogin>(b =>
        {
            b.HasKey(l => new { l.LoginProvider, l.ProviderKey });

            if (maxKeyLength > 0)
            {
                b.Property(l => l.LoginProvider).HasMaxLength(maxKeyLength);
                b.Property(l => l.ProviderKey).HasMaxLength(maxKeyLength);
            }

            //b.ToTable("AspNetUserLogins");
        });

        builder.Entity<TUserToken>(b =>
        {
            b.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

            if (maxKeyLength > 0)
            {
                b.Property(t => t.LoginProvider).HasMaxLength(maxKeyLength);
                b.Property(t => t.Name).HasMaxLength(maxKeyLength);
            }

            if (encryptPersonalData)
            {
                var tokenProps = typeof(TUserToken).GetProperties().Where(
                                prop => Attribute.IsDefined(prop, typeof(ProtectedPersonalDataAttribute)));
                foreach (var p in tokenProps)
                {
                    if (p.PropertyType != typeof(string))
                    {
                        throw new InvalidOperationException(CanOnlyProtectStrings);
                    }
                    b.Property(typeof(string), p.Name).HasConversion(converter);
                }
            }

            //b.ToTable("AspNetUserTokens");
        });
    }

    /// <summary>
    /// Configures the schema needed for the identity framework for schema version 2.0
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="accessor"></param>
    /// <param name="builder">
    /// The builder being used to construct the model for this context.
    /// </param>
    private static void OnModelCreatingVersion2<
        [DynamicallyAccessedMembers(IEntity.DAMT)] TUser,
        [DynamicallyAccessedMembers(IEntity.DAMT)] TRole,
        [DynamicallyAccessedMembers(IEntity.DAMT)] TKey,
        [DynamicallyAccessedMembers(IEntity.DAMT)] TUserClaim,
        [DynamicallyAccessedMembers(IEntity.DAMT)] TUserRole,
        [DynamicallyAccessedMembers(IEntity.DAMT)] TUserLogin,
        [DynamicallyAccessedMembers(IEntity.DAMT)] TRoleClaim,
        [DynamicallyAccessedMembers(IEntity.DAMT)] TUserToken>(
        string prefix,
        IInfrastructure<IServiceProvider> accessor, ModelBuilder builder)
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TUserLogin : IdentityUserLogin<TKey>
        where TRoleClaim : IdentityRoleClaim<TKey>
        where TUserToken : IdentityUserToken<TKey>
    {
        OnModelCreatingVersion2<TUser, TKey, TUserClaim, TUserLogin, TUserToken>(prefix, accessor, builder);

        // Current no differences between Version 2 and Version 1
        builder.Entity<TUser>(b =>
        {
            b.HasMany<TUserRole>().WithOne().HasForeignKey(ur => ur.UserId).IsRequired();
        });

        builder.Entity<TRole>(b =>
        {
            b.HasKey(r => r.Id);
            b.HasIndex(r => r.NormalizedName).HasDatabaseName(prefix + "RoleNameIndex").IsUnique();
            //b.ToTable("AspNetRoles");
            b.Property(r => r.ConcurrencyStamp).IsConcurrencyToken();

            b.Property(u => u.Name).HasMaxLength(256);
            b.Property(u => u.NormalizedName).HasMaxLength(256);

            b.HasMany<TUserRole>().WithOne().HasForeignKey(ur => ur.RoleId).IsRequired();
            b.HasMany<TRoleClaim>().WithOne().HasForeignKey(rc => rc.RoleId).IsRequired();
        });

        builder.Entity<TRoleClaim>(b =>
        {
            b.HasKey(rc => rc.Id);
            //b.ToTable("AspNetRoleClaims");
        });

        builder.Entity<TUserRole>(b =>
        {
            b.HasKey(r => new { r.UserId, r.RoleId });
            //b.ToTable("AspNetUserRoles");
        });
    }

    protected static void OnModelCreatingVersion2(IInfrastructure<IServiceProvider> accessor, ModelBuilder builder)
        => OnModelCreatingVersion2<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>("AC", accessor, builder);

    /// <summary>
    /// https://github.com/dotnet/aspnetcore/blob/v9.0.8/src/Identity/EntityFrameworkCore/src/Resources.resx#L120
    /// </summary>
    private const string CanOnlyProtectStrings = "[ProtectedPersonalData] only works strings by default.";
}

file sealed class PersonalDataConverter : ValueConverter<string, string>
{
    public PersonalDataConverter(IPersonalDataProtector protector) : base(s => protector.Protect(s), s => protector.Unprotect(s), default)
    { }
}