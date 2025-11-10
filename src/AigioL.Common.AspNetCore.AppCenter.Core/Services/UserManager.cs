using AigioL.Common.AspNetCore.AppCenter.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AigioL.Common.AspNetCore.AppCenter.Services;

/// <inheritdoc/>
public partial class UserManager :
    UserManager<User>
{
    protected readonly IHttpContextAccessor accessor;

    /// <inheritdoc/>
#pragma warning disable IDE0290 // 使用主构造函数
    public UserManager(
#pragma warning restore IDE0290 // 使用主构造函数
        IUserStore<User> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<User> passwordHasher,
        IEnumerable<IUserValidator<User>> userValidators,
        IEnumerable<IPasswordValidator<User>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager> logger) : base(
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
