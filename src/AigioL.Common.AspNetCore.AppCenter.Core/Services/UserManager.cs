using AigioL.Common.AspNetCore.AppCenter.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AigioL.Common.AspNetCore.AppCenter.Services;

/// <inheritdoc/>
public partial class UserManager :
    UserManager<User>
{
    protected readonly ILogger logger;
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
        this.logger = logger;
        accessor = services.GetRequiredService<IHttpContextAccessor>();
    }

    /// <inheritdoc/>
    public override string? GetUserId(ClaimsPrincipal principal)
    {
        var context = accessor.HttpContext;
        if (context != null)
        {
            if (context.User == principal)
            {
                // 此处重写此逻辑适配 jwtId 到 userId 的转换
                var userId = context.GetUserIdThrowIfNull();
                return userId.ToString();
            }
        }

        return base.GetUserId(principal);
    }
}
