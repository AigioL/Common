using AigioL.Common.AspNetCore.AppCenter.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AigioL.Common.AspNetCore.AppCenter.Repositories;

/// <inheritdoc/>
public sealed partial class UserManager2 :
    UserManager<User>
{
    readonly IHttpContextAccessor accessor;

    /// <inheritdoc/>
    public UserManager2(
        IUserStore<User> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<User> passwordHasher,
        IEnumerable<IUserValidator<User>> userValidators,
        IEnumerable<IPasswordValidator<User>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<User>> logger) : base(
            store,
            optionsAccessor,
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            services,
            logger)
    {
        accessor = services.GetRequiredService<IHttpContextAccessor>();
    }

    /// <inheritdoc/>
    public sealed override string? GetUserId(ClaimsPrincipal principal)
    {
        var ctx = accessor.HttpContext;
        if (ctx != null)
        {
            if (ctx.User == principal)
            {
                // 重写此逻辑适配 jwt id 到 user id 的转换
                var userId = ctx.GetUserIdThrowIfNull();
                return userId.ToString();
            }
        }

        return base.GetUserId(principal);
    }
}
