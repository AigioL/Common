using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.JsonWebTokens.Models.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionServiceExtensions
{
    public static IServiceCollection AddIdentityRepositories<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext, IIdentityDbContext
    {
        services.TryAddScoped<IAuthMessageRecordRepository, AuthMessageRecordRepository<TDbContext>>();
        //services.TryAddScoped<IClockInRecordRepository, ClockInRecordRepository<TDbContext>>();
        services.TryAddScoped<IUserDeleteRepository, UserDeleteRepository<TDbContext>>();
        services.TryAddScoped<IUserMembershipRepository, UserMembershipRepository<TDbContext>>();
        return services;
    }

    /// <summary>
    /// 添加由 <see cref="IUserManager2"/> 实现的用户管理服务
    /// </summary>
    public static IServiceCollection AddACUserManager2<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext>(
        this IServiceCollection services)
        where TOptions : class, IJsonWebTokenOptions
        where TDbContext : DbContext, IIdentityDbContext
    {
        services.AddScoped<IIdentityJsonWebTokenValueProvider, IdentityJsonWebTokenValueProvider<TOptions, TDbContext>>();
        services.AddScoped<UserManager2<TDbContext>>();
        services.AddScoped<IUserManager2>(static s => s.GetRequiredService<UserManager2<TDbContext>>());
        services.AddScoped<UserManager<User>>(static s => s.GetRequiredService<UserManager2<TDbContext>>());
        services.AddScoped<IUserValidator<User>, UserValidator2<User>>();
        return services;
    }
}

/// <summary>
/// Provides validation services for user classes.
/// </summary>
/// <typeparam name="TUser">The type encapsulating a user.</typeparam>
/// <remarks>
/// Creates a new instance of <see cref="UserValidator2{TUser}"/>.
/// </remarks>
/// <param name="errors">The <see cref="IdentityErrorDescriber"/> used to provider error messages.</param>
file sealed class UserValidator2<TUser>(IdentityErrorDescriber? errors = null) : IUserValidator<TUser> where TUser : class
{
    /// <summary>
    /// Gets the <see cref="IdentityErrorDescriber"/> used to provider error messages for the current <see cref="UserValidator2{TUser}"/>.
    /// </summary>
    /// <value>The <see cref="IdentityErrorDescriber"/> used to provider error messages for the current <see cref="UserValidator2{TUser}"/>.</value>
    public IdentityErrorDescriber Describer { get; private set; } = errors ?? new IdentityErrorDescriber();

    /// <summary>
    /// Validates the specified <paramref name="user"/> as an asynchronous operation.
    /// </summary>
    /// <param name="manager">The <see cref="UserManager{TUser}"/> that can be used to retrieve user properties.</param>
    /// <param name="user">The user to validate.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the validation operation.</returns>
    public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(user);
        var errors = await ValidateUserName(manager, user).ConfigureAwait(false);
        if (manager.Options.User.RequireUniqueEmail)
        {
            errors = await ValidateEmail(manager, user, errors).ConfigureAwait(false);
        }
        return errors?.Count > 0 ? IdentityResult.Failed([.. errors]) : IdentityResult.Success;
    }

    async Task<List<IdentityError>?> ValidateUserName(UserManager<TUser> manager, TUser user)
    {
        List<IdentityError>? errors = null;
        var userName = await manager.GetUserNameAsync(user).ConfigureAwait(false);
        if (userName != null) // 允许使用空用户名
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                errors ??= new List<IdentityError>();
                errors.Add(Describer.InvalidUserName(userName));
            }
            else if (!string.IsNullOrEmpty(manager.Options.User.AllowedUserNameCharacters) &&
                userName.Any(c => !manager.Options.User.AllowedUserNameCharacters.Contains(c)))
            {
                errors ??= new List<IdentityError>();
                errors.Add(Describer.InvalidUserName(userName));
            }
            else
            {
                var owner = await manager.FindByNameAsync(userName).ConfigureAwait(false);
                if (owner != null &&
                    !string.Equals(await manager.GetUserIdAsync(owner).ConfigureAwait(false), await manager.GetUserIdAsync(user).ConfigureAwait(false)))
                {
                    errors ??= new List<IdentityError>();
                    errors.Add(Describer.DuplicateUserName(userName));
                }
            }
        }

        return errors;
    }

    // make sure email is not empty, valid, and unique
    private async Task<List<IdentityError>?> ValidateEmail(UserManager<TUser> manager, TUser user, List<IdentityError>? errors)
    {
        var email = await manager.GetEmailAsync(user).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(email))
        {
            errors ??= new List<IdentityError>();
            errors.Add(Describer.InvalidEmail(email));
            return errors;
        }
        if (!new EmailAddressAttribute().IsValid(email))
        {
            errors ??= new List<IdentityError>();
            errors.Add(Describer.InvalidEmail(email));
            return errors;
        }
        var owner = await manager.FindByEmailAsync(email).ConfigureAwait(false);
        if (owner != null &&
            !string.Equals(await manager.GetUserIdAsync(owner).ConfigureAwait(false), await manager.GetUserIdAsync(user).ConfigureAwait(false)))
        {
            errors ??= new List<IdentityError>();
            errors.Add(Describer.DuplicateEmail(email));
        }
        return errors;
    }
}