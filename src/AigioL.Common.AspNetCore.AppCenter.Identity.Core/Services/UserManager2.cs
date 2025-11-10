using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Response;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Services;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Services;

/// <inheritdoc/>
sealed partial class UserManager2<TDbContext> : UserManager
   where TDbContext : DbContext, IIdentityDbContext
{
    readonly TDbContext db;

    /// <inheritdoc/>
#pragma warning disable IDE0290 // 使用主构造函数
    public UserManager2(
#pragma warning restore IDE0290 // 使用主构造函数
        TDbContext db,
        IUserStore<User> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<User> passwordHasher,
        IEnumerable<IUserValidator<User>> userValidators,
        IEnumerable<IPasswordValidator<User>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager2<TDbContext>> logger) : base(
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
        this.db = db;
    }
}

partial class UserManager2<TDbContext> : IIdentityUserManager<User>
{
    /// <inheritdoc/>
    UserManager<User> IIdentityUserManager<User>.Impl => this;

    /// <inheritdoc/>
    public async Task<IdentityResult> SetPhoneNumberAsync(User user, string? phoneNumber, string? phoneNumberRegionCode)
    {
        user.PhoneNumberRegionCode = phoneNumberRegionCode;
        var r = await SetPhoneNumberAsync(user, phoneNumber);
        return r;
    }

    /// <inheritdoc/>
    public async Task<User?> FindByIdAsync(Guid id)
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
    public async new Task<IdentityResult> UpdateUserAsync(User user)
    {
        var r = await base.UpdateUserAsync(user);
        return r;
    }

    /// <inheritdoc/>
    public async Task<User?> FindByPhoneNumberAsync(string phoneNumber, string? regionCode)
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
    public async Task<User?> FindByTokenIdAsync(Guid jwtId)
    {
        var cancellationToken = CancellationToken;
        // https://github.com/dotnet/aspnetcore/blob/v5.0.3/src/Identity/EntityFrameworkCore/src/UserStore.cs#L234
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var query = from m in db.UserJsonWebTokens
                    .Include(x => x.UserDevice)
                        .ThenInclude(x => x.User)
                    where m.Id == jwtId && m.UserDevice != null && m.UserDevice.User != null
                    select m.UserDevice.User;

        var user = await query.FirstOrDefaultAsync(cancellationToken);
        return user;
    }

    /// <inheritdoc/>
    public async Task<JsonWebTokenValue?> RefreshTokenAsync(
        DevicePlatform2 platform,
        string? deviceId,
        string refresh_token)
    {
        throw new NotImplementedException("TODO");
    }

    /// <inheritdoc/>
    public async Task<User?> FindByAccountAsync(string account)
    {
        throw new NotImplementedException("TODO");
    }

    /// <inheritdoc/>
    public async Task<User?> GetUserAsync()
    {
        throw new NotImplementedException("TODO");
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("TODO");
    }

    /// <inheritdoc/>
    public async Task RefreshUserInfoCacheAsync(User user)
    {
        throw new NotImplementedException("TODO");
    }

    /// <inheritdoc/>
    public async Task RefreshUserInfoCacheAsync(UserInfoModel user)
    {
        throw new NotImplementedException("TODO");
    }
}

partial class UserManager2<TDbContext> : IUserManager2
{
    /// <inheritdoc/>
    public async Task<UserType> GetUserTypeByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("TODO");
    }

    /// <inheritdoc/>
    public async Task<UserInfoModel?> GetUserInfoCacheAsync(bool isOpenId = false)
    {
        throw new NotImplementedException("TODO");
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<LoginOrRegisterResponse?>> LoginSharedAsync(
        User user,
        bool isLoginOrRegister,
        string? deviceId)
    {
        throw new NotImplementedException("TODO");
    }

    /// <inheritdoc/>
    public async Task UnbundleAccountAsync(User user, ExternalLoginChannel channel)
    {
        throw new NotImplementedException("TODO");
    }

    #region 创建用户

    /// <inheritdoc/>
    public async Task<(User user, IdentityResult identityResult)> CreateByPhoneNumberAsync(
        string phoneNumber,
        string? regionCode,
        bool phoneNumberConfirmed)
    {
        throw new NotImplementedException("TODO");
    }

    /// <inheritdoc/>
    public async Task<(User user, IdentityResult identityResult)> CreateByEmailAsync(
        string email,
        string password,
        bool emailConfirmed)
    {
        throw new NotImplementedException("TODO");
    }

    #endregion
}