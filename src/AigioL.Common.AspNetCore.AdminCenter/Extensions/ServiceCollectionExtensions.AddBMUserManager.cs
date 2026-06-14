using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加由 <see cref="IIdentityUserManager{TUser}"/> 实现的用户管理服务
    /// </summary>
    public static IServiceCollection AddBMUserManager<
        TUser, TRole, TUserClaim,
        TUserRole, TUserLogin, TRoleClaim,
        TUserToken,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext>(
        this IServiceCollection services)
        where TUser : IdentityUser<Guid>, IPhoneNumber
        where TRole : IdentityRole<Guid>
        where TUserClaim : IdentityUserClaim<Guid>
        where TUserRole : IdentityUserRole<Guid>, ITenantId
        where TUserLogin : IdentityUserLogin<Guid>
        where TRoleClaim : IdentityRoleClaim<Guid>
        where TUserToken : IdentityUserToken<Guid>
        where TDbContext : DbContext, IIdentityDbContext<TUser, TRole, Guid, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
    {
        services.AddScoped<BMUserManager<TUser, TRole, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken, TDbContext>>();
        services.AddScoped<IIdentityUserManager<TUser>>(static s => s.GetRequiredService<BMUserManager<TUser, TRole, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken, TDbContext>>());
        services.AddScoped<UserManager<TUser>>(static s => s.GetRequiredService<BMUserManager<TUser, TRole, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken, TDbContext>>());
        //services.AddScoped<IUserValidator<User>, UserValidator2<User>>();
        return services;
    }
}

file sealed partial class BMUserManager<
    TUser, TRole, TUserClaim,
    TUserRole, TUserLogin, TRoleClaim,
    TUserToken,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext>(
    TDbContext db,
    IHttpContextAccessor accessor,
    IUserStore<TUser> store,
    IOptions<IdentityOptions> optionsAccessor,
    IPasswordHasher<TUser> passwordHasher,
    IEnumerable<IUserValidator<TUser>> userValidators,
    IEnumerable<IPasswordValidator<TUser>> passwordValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    IServiceProvider services,
    ILogger<BMUserManager<TUser, TRole, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken, TDbContext>> logger) :
    UserManager<TUser>(store, optionsAccessor, passwordHasher,
        userValidators, passwordValidators, keyNormalizer,
        errors, services, logger),
    IIdentityUserManager<TUser>
    where TUser : IdentityUser<Guid>, IPhoneNumber
    where TRole : IdentityRole<Guid>
    where TUserClaim : IdentityUserClaim<Guid>
    where TUserRole : IdentityUserRole<Guid>, ITenantId
    where TUserLogin : IdentityUserLogin<Guid>
    where TRoleClaim : IdentityRoleClaim<Guid>
    where TUserToken : IdentityUserToken<Guid>
    where TDbContext : DbContext, IIdentityDbContext<TUser, TRole, Guid, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
{
    /// <inheritdoc/>
    UserManager<TUser> IIdentityUserManager<TUser>.Impl => this;

    /// <inheritdoc/>
    public new Guid? GetUserId(ClaimsPrincipal? principal)
    {
        if (principal == null)
        {
            return null;
        }

        var userId = base.GetUserId(principal);
        if (ShortGuid.TryParse(userId, out Guid userIdG))
        {
            return userIdG;
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<IdentityResult> SetPhoneNumberAsync(TUser user, string? phoneNumber, string? phoneNumberRegionCode)
    {
        user.PhoneNumberRegionCode = phoneNumberRegionCode;
        var r = await SetPhoneNumberAsync(user, phoneNumber);
        return r;
    }

    /// <inheritdoc/>
    public async Task<TUser?> FindByIdAsync(Guid id)
    {
        var cancellationToken = CancellationToken;
        // https://github.com/dotnet/aspnetcore/blob/v5.0.3/src/Identity/EntityFrameworkCore/src/UserStore.cs#L234
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var query = from m in db.Users
                    where m.Id == id
                    select m;

        var user = await query.FirstOrDefaultAsync(cancellationToken);
        return user;
    }

    /// <inheritdoc/>
    public async new Task<IdentityResult> UpdateUserAsync(TUser user)
    {
        var r = await base.UpdateUserAsync(user);
        return r;
    }

    /// <inheritdoc/>
    public async Task<TUser?> FindByPhoneNumberAsync(string phoneNumber, string? regionCode)
    {
        var cancellationToken = CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var query = from m in db.Users
                    where m.PhoneNumber == phoneNumber && m.PhoneNumberRegionCode == regionCode
                    select m;

        var user = await query.FirstOrDefaultAsync(cancellationToken);
        return user;
    }

    /// <inheritdoc/>
    public async Task<TUser?> GetUserAsync()
    {
        var context = accessor.HttpContext;
        if (context != null)
        {
            var userId = context.GetUserId();
            if (userId.HasValue)
            {
                var user = await FindByIdAsync(userId.Value);
                return user;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<TUser?> FindByAccountAsync(string account)
    {
        if (string.IsNullOrWhiteSpace(account))
        {
            return null;
        }

        if (account.StartsWith("+86") && account.Length == 14)
        {
            var r = await FindByPhoneNumberAsync(account[3..], "+86");
            return r;
        }
        else if (account.Length == 11 && account.All(char.IsAsciiDigit))
        {
            var r = await FindByPhoneNumberAsync(account, "+86");
            return r;
        }
        if (new EmailAddressAttribute().IsValid(account))
        {
            var r = await FindByEmailAsync(account);
            return r;
        }
        else
        {
            var r = await FindByNameAsync(account);
            return r;
        }
    }

    /// <inheritdoc/>
    public async Task<int> UpdateTenantIdAsync(Guid userId, Guid tenantId)
    {
        var r = await db.UserRoles
            .Where(x => x.UserId == userId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.TenantId, tenantId));
        return r;
    }
}