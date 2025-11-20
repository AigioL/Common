using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Response;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Services;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;
using MemoryPack;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using R = AigioL.Common.AspNetCore.AppCenter.Identity.UI.Properties.Resources;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Services;

/// <inheritdoc/>
sealed partial class UserManager2<TDbContext> : UserManager
   where TDbContext : DbContext, IIdentityDbContext
{
    readonly TDbContext db;
    readonly IIdentityJsonWebTokenValueProvider jwtValueProvider;
    readonly IConnectionMultiplexer connection;

    /// <inheritdoc/>
#pragma warning disable IDE0290 // 使用主构造函数
    public UserManager2(
#pragma warning restore IDE0290 // 使用主构造函数
        IConnectionMultiplexer connection,
        TDbContext db,
        IIdentityJsonWebTokenValueProvider jwtValueProvider,
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
        this.jwtValueProvider = jwtValueProvider;
        this.connection = connection;
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

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "移除当前 JWT 与生成一个新的 JWT 时出错！UserId: {userId}, DevicePlatform2: {platform}, DeviceId: {deviceId}")]
    private static partial void LogErrorOnRefreshTokenRemoveWithGenerateNew(ILogger logger, Exception? ex,
        Guid userId, DevicePlatform2 platform, string? deviceId);

    /// <inheritdoc/>
    public async Task<JsonWebTokenValue?> RefreshTokenAsync(
        DevicePlatform2 platform,
        string? deviceId,
        string refresh_token)
    {
        var cancellationToken = CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();
        var jwtR_entity = await db.UserRefreshJsonWebTokens
            .FirstOrDefaultAsync(x => x.RefreshToken == refresh_token, cancellationToken);
        if (jwtR_entity != null)
        {
            if (jwtR_entity.Version == 0)
            {
                jwtR_entity.NotBefore = jwtR_entity.NotBefore.AddDays(-60);
            }
            var now = DateTimeOffset.Now;
            if (now >= jwtR_entity.NotBefore && jwtR_entity.RefreshExpiration >= now)
            {
                var jwt_entity = await db.UserJsonWebTokens
                    .Include(x => x.UserDevice)
                    .FirstOrDefaultAsync(x => x.UserDevice != null && x.Id == jwtR_entity.Id,
                        cancellationToken: cancellationToken);
                if (jwt_entity != null)
                {
                    var user = await FindByIdAsync(jwt_entity.UserDevice.UserId);
                    if (user != null)
                    {
                        var roles = await GetRolesAsync(user);
                        using var transaction = db.GetDatabase().BeginTransaction();
                        try
                        {
                            // 移除当前 JWT
                            db.UserRefreshJsonWebTokens.Remove(jwtR_entity);
                            db.UserJsonWebTokens.Remove(jwt_entity);
                            await db.SaveChangesAsync();

                            // 生成一个新的 JWT
                            var jwt = await jwtValueProvider.GenerateTokenAsync(
                                jwt_entity.UserDevice.UserId,
                                platform, deviceId, roles,
                                cancellationToken: cancellationToken);

                            await transaction.CommitAsync();
                            return jwt.jwtData;
                        }
                        catch (Exception ex)
                        {
                            LogErrorOnRefreshTokenRemoveWithGenerateNew(logger, ex, user.Id, platform, deviceId);
                        }
                    }
                }
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<User?> FindByAccountAsync(string account)
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
    public async Task<User?> GetUserAsync()
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
    public async Task<bool> ExistsEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // https://github.com/dotnet/aspnetcore/blob/v5.0.3/src/Identity/EntityFrameworkCore/src/UserStore.cs#L234
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        if (string.IsNullOrEmpty(email))
        {
            return false;
        }

        var email2 = NormalizeEmail(email);

        var query = from m in db.Users.AsNoTrackingWithIdentityResolution()
                    where m.NormalizedEmail == email2
                    select m;

        var r = await query.AnyAsync(cancellationToken);
        return r;
    }

    /// <inheritdoc/>
    public async Task RefreshUserInfoCacheAsync(User user)
    {
        var userInfo = await GetUserInfoCacheAsync(user);
        await RefreshUserInfoCacheAsync(userInfo);
    }

    /// <inheritdoc/>
    public async Task RefreshUserInfoCacheAsync(UserInfoModel userInfo)
    {
        var redisDb = connection.GetDatabase(CacheKeys.RedisHashDataDb);
        var hashKey = ShortGuid.Encode(userInfo.Id);
        var hashValue = MemoryPackSerializer.Serialize(userInfo);
        await redisDb.HashSetAsync(CacheKeys.IdentityUserInfoDataHashV1Key, hashKey, hashValue);
    }
}

partial class UserManager2<TDbContext> : IUserManager2
{
    /// <inheritdoc/>
    public async Task<UserType> GetUserTypeByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var query = from m in db.Users
                    where m.Id == userId
                    select m.UserType;

        var userType = await query.FirstOrDefaultAsync(cancellationToken);
        return userType;
    }

    /// <inheritdoc/>
    public async Task<UserInfoModel?> GetUserInfoCacheAsync()
    {
        var ctx = accessor.HttpContext;
        if (ctx != null)
        {
            var userId = ctx.GetUserId();
            if (userId.HasValue)
            {
                var redisDb = connection.GetDatabase(CacheKeys.RedisHashDataDb);
                var hashKey = ShortGuid.Encode(userId.Value);
                var cacheData = await redisDb.HashGetAsync(CacheKeys.IdentityUserInfoDataHashV1Key, hashKey);
                if (cacheData.HasValue)
                {
                    var userInfo = MemoryPackSerializer.Deserialize<UserInfoModel>((byte[])cacheData!);
                    if (userInfo != null)
                    {
                        return userInfo;
                    }
                }
                else
                {
                    var user = await GetUserAsync();
                    if (user != null)
                    {
                        var userInfo = await GetUserInfoCacheAsync(user);
                        await RefreshUserInfoCacheAsync(userInfo);
                        return userInfo;
                    }
                }
            }
        }
        return null;
    }

    internal static Dictionary<ExternalLoginChannel, string> GetAvatarUrl(User user)
    {
        var dict = new Dictionary<ExternalLoginChannel, string>();

        var avatars = user.ExternalAccounts!
            .Where(x => !string.IsNullOrEmpty(x.AvatarUrl))
            .Select(a => new { a.Type, a.AvatarUrl })
            .ToArray();
        foreach (var avatar in avatars)
        {
            if (avatar.AvatarUrl.IsHttpUrl() && !dict.ContainsKey(avatar.Type))
            {
                dict.Add(avatar.Type, avatar.AvatarUrl);
            }
        }
        return dict;
    }

    public async Task<UserInfoModel> GetUserInfoCacheAsync(User user)
    {
        var cancellationToken = CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        await db.Entry(user).Reference(x => x.Wallet).LoadAsync(cancellationToken);
        await db.Entry(user).Collection(x => x.ExternalAccounts).LoadAsync(cancellationToken);

        //// 获取下级升级所需经验和当前等级
        //var (level, nextExperience) = User.GetLevel((uint)user.Experience);

        //var lastSignInTime = db.UserClockInRecords.Where(x => x.UserId == user.Id)
        //    .OrderByDescending(x => x.CreationTime)
        //    .Select(x => x.CreationTime)
        //    .FirstOrDefault();

        return new()
        {
            Id = user.Id,
            NickName = user.GetNickName(),
            Avatar = Guid.Empty,
            //Experience = (uint)user.Experience,
            Balance = user.Wallet.AccountBalance,
            //Level = level,
            PersonalizedSignature = user.PersonalizedSignature,
            PhoneNumber = user.PhoneNumber,
            SteamAccountId = long.TryParse(user.ExternalAccounts!.FirstOrDefault(a => a.Type == ExternalLoginChannel.Steam)?.ExternalAccountId, out var steamId) ? steamId : null,
            Gender = user.Gender,
            BirthDate = user.BirthDate.HasValue ? user.BirthDate.Value.DateTime : null,
            //BirthDateTimeZone = user.BirthDateTimeZone,
            // 当前登录用户不返回计算年龄值，由客户端本地计算
            AreaId = user.AreaId,
            MicrosoftAccountEmail = user.ExternalAccounts!.FirstOrDefault(a => a.Type == ExternalLoginChannel.Microsoft)?.Email ?? string.Empty,
            QQNickName = user.ExternalAccounts!.FirstOrDefault(a => a.Type == ExternalLoginChannel.QQ)?.NickName,
            AvatarUrl = GetAvatarUrl(user),
            UserType = user.UserType,
            //IsSignIn = lastSignInTime != default && lastSignInTime.Date.AddDays(1) > DateTimeOffset.UtcNow,
            //NextExperience = nextExperience,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            HasPassword = await HasPasswordAsync(user),
            PhoneNumberRegionCode = user.PhoneNumberRegionCode,
        };
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<LoginOrRegisterResponse?>> LoginSharedAsync(
        User user,
        bool isLoginOrRegister,
        string? deviceId)
    {
        ArgumentNullException.ThrowIfNull(user);

        var httpContext = accessor.HttpContext;
        ArgumentNullException.ThrowIfNull(httpContext);

        var cancellationToken = CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var isLocked = await IsLockedOutAsync(user);
        if (isLocked) // 账号被封禁
        {
            return ApiRspCode.UserIsBanOrLock;
        }

        var roles = await GetRolesAsync(user);
        var platform = httpContext.GetDevicePlatform();
        var (jwtData, jwtId) = await jwtValueProvider.GenerateTokenAsync(user.Id, platform, deviceId, roles, cancellationToken: cancellationToken);
        var userInfo = await GetUserInfoCacheAsync(user);

        // 写入缓存
        await RefreshUserInfoCacheAsync(userInfo);

        return new LoginOrRegisterResponse
        {
            IsLoginOrRegister = isLoginOrRegister,
            User = userInfo,
            AuthToken = jwtData,
            PhoneNumber = user.PhoneNumber,
        };
    }

    /// <inheritdoc/>
    public async Task UnbundleAccountAsync(User user, ExternalLoginChannel channel)
    {
        var query = from a in db.ExternalAccounts
                    where a.Type == channel && a.UserId == user.Id
                    select a;
        var externalAccountIds = await query.Select(static a => a.ExternalAccountId).ToArrayAsync();

        var redisDb = connection.GetDatabase(CacheKeys.RedisHashDataDb);
        foreach (var externalAccountId in externalAccountIds)
        {
            await redisDb.HashDeleteAsync($"{CacheKeys.IdentityUserExternalAccountsHashKey}_C_{channel}", externalAccountId);
        }

        await query.ExecuteDeleteAsync();
    }

    /// <inheritdoc/>
    public async Task<ApiRsp<LoginOrRegisterResponse?>> LoginOrRegisterOrBindAsync(
        string externalAccountId,
        ExternalLoginChannel channel,
        string deviceId,
        Guid? bindUserId = null,
        Action<ExternalAccount>? setProperties = null)
    {
        CancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        Guid userId;
        var redisDb = connection.GetDatabase(CacheKeys.RedisHashDataDb);
        var userIdCache = await redisDb.HashGetAsync($"{CacheKeys.IdentityUserExternalAccountsHashKey}_C_{channel}", externalAccountId);
        if (userIdCache.HasValue)
        {
            userId = new Guid((byte[])userIdCache!);
            if (userId == default)
            {
                userId = await db.Users.Include(x => x.ExternalAccounts)
                .Where(x => x.ExternalAccounts != null &&
                     x.ExternalAccounts.Any(y => y.ExternalAccountId == externalAccountId &&
                         y.Type == channel))
                 .Select(x => x.Id).FirstOrDefaultAsync();
            }
        }
        else
        {
            userId = await db.Users.Include(x => x.ExternalAccounts)
                .Where(x => x.ExternalAccounts != null &&
                     x.ExternalAccounts.Any(y => y.ExternalAccountId == externalAccountId &&
                         y.Type == channel))
                 .Select(x => x.Id).FirstOrDefaultAsync();
        }
        if (userId == default) // Register
        {
            var externalAccount = await db.ExternalAccounts
                .OrderBy(x => x.CreationTime)
                .FirstOrDefaultAsync(a => a.ExternalAccountId == externalAccountId &&
                    a.Type == channel);
            externalAccount ??= new ExternalAccount
            {
                ExternalAccountId = externalAccountId,
                Type = channel,
            };
            setProperties?.Invoke(externalAccount);
            if (bindUserId.HasValue) // 绑定用户
            {
                var bindUser = await FindByIdAsync(bindUserId.Value);
                ArgumentNullException.ThrowIfNull(bindUser);
                SetUserPropertiesByExternalAccount(bindUser, externalAccount);

                externalAccount.UserId = bindUserId;
                if (externalAccount.Id == default)
                {
                    await db.ExternalAccounts.AddAsync(externalAccount);
                }
                await db.SaveChangesAsync();
                await redisDb.HashSetAsync($"{CacheKeys.IdentityUserExternalAccountsHashKey}_C_{channel}", externalAccountId, bindUserId.Value.ToByteArray());
                await RefreshUserInfoCacheAsync(bindUser);
                return GetBindRsp(externalAccount);
            }
            else // 创建用户
            {
                (var user, var result) = await CreateAccountAsync(externalAccount);
                if (result.Succeeded)
                {
                    var r = await LoginSharedAsync(user, false, deviceId);
                    await redisDb.HashSetAsync($"{CacheKeys.IdentityUserExternalAccountsHashKey}_C_{channel}", externalAccountId, user.Id.ToByteArray());
                    return r;
                }
                const ApiRspCode code = ApiRspCode.BadRequest;
                return result.Fail<LoginOrRegisterResponse>(code);
            }
        }
        else // Login
        {
            if (bindUserId == userId)
            {
                var externalAccount = await UpdateExternalAccountAsync(externalAccountId,
                    channel, userId, setProperties);
                setProperties?.Invoke(externalAccount);
                return GetBindRsp(externalAccount);
            }
            else if (bindUserId != default) // 该外部账号已被 userId 使用不可绑定
            {
                return R.BindFail_UserIsNotNull;
            }
            else // 更新外部账号
            {
                var externalAccount = await UpdateExternalAccountAsync(externalAccountId, channel, userId, setProperties);

                var loginUser = await FindByIdAsync(userId);
                ArgumentNullException.ThrowIfNull(loginUser);

                SetUserPropertiesByExternalAccount(loginUser, externalAccount);

                var r = await LoginSharedAsync(loginUser, true, deviceId);
                return r;
            }
        }
    }

    static LoginOrRegisterResponse GetBindRsp(ExternalAccount externalAccount)
    {
        // 绑定快速登录账号时，将第三方平台可覆盖的信息，例如昵称，性别等，直接赋值在 DTO 上，由客户端比较原值为空时覆盖
        var user = new UserInfoModel
        {
            NickName = externalAccount.NickName ?? string.Empty,
        };

        switch (externalAccount.Type)
        {
            case ExternalLoginChannel.Microsoft:
                if (!string.IsNullOrWhiteSpace(externalAccount.Email))
                    user.MicrosoftAccountEmail = externalAccount.Email;
                break;
            case ExternalLoginChannel.Apple:
                if (!string.IsNullOrWhiteSpace(externalAccount.Email))
                    user.AppleAccountEmail = externalAccount.Email;
                break;
            case ExternalLoginChannel.QQ:
                user.QQNickName = externalAccount.NickName;
                break;
            case ExternalLoginChannel.Steam:
                user.SteamAccountId = long.TryParse(externalAccount.ExternalAccountId,
                    out var steamAccountId) ? steamAccountId : null;
                break;
        }

        if (Enum.IsDefined(externalAccount.Gender) && externalAccount.Gender != Gender.Unknown)
        {
            user.Gender = externalAccount.Gender;
        }

        if (!string.IsNullOrWhiteSpace(externalAccount.AvatarUrl))
        {
            user.AvatarUrl = new Dictionary<ExternalLoginChannel, string>
            {
                { externalAccount.Type, externalAccount.AvatarUrl },
            };
        }

        var r = new LoginOrRegisterResponse()
        {
            User = user,
        };
        return r;
    }

    static void SetUserPropertiesByExternalAccount(User user, ExternalAccount externalAccount)
    {
        if ((string.IsNullOrEmpty(user.NickName) || user.IsGeneratedNickName())
            && !string.IsNullOrWhiteSpace(externalAccount.NickName))
        {
            user.NickName = externalAccount.NickName;
        }

        if (user.Gender == default
            && Enum.IsDefined(externalAccount.Gender) && externalAccount.Gender != Gender.Unknown)
        {
            user.Gender = externalAccount.Gender;
        }

        if (user.AvatarUrl == default
            && !string.IsNullOrWhiteSpace(externalAccount.AvatarUrl)
            && externalAccount.AvatarUrl.IsHttpUrl())
        {
            user.AvatarUrl = externalAccount.AvatarUrl;
        }
    }

    async Task<(User user, IdentityResult identityResult)> CreateAccountAsync(ExternalAccount externalAccount)
    {
        var user = new User
        {
            ExternalAccounts = new() { externalAccount }
        };
        SetUserPropertiesByExternalAccount(user, externalAccount);
        var identityResult = await CreateAsync(user);
        return (user, identityResult);
    }

    /// <summary>
    /// 更新外部账号
    /// </summary>
    async Task<ExternalAccount> UpdateExternalAccountAsync(
        string externalAccountId,
        ExternalLoginChannel channel,
        Guid userId = default,
        Action<ExternalAccount>? setProperties = null)
    {
        var externalAccounts = await db.ExternalAccounts
                    .Where(x => (x.UserId.HasValue && x.UserId == userId) &&
                        x.ExternalAccountId == externalAccountId &&
                        x.Type == channel)
                    .OrderBy(x => x.CreationTime)
                    .ToArrayAsync();
        if (externalAccounts != null && externalAccounts.Length != 0)
        {
            var externalAccount = externalAccounts.First();
            setProperties?.Invoke(externalAccount);
            if (externalAccounts.Length > 1)
            {
                db.ExternalAccounts.RemoveRange(externalAccounts.Skip(1));
            }
            await db.SaveChangesAsync();
            return externalAccount;
        }
        else
        {
            var externalAccount = new ExternalAccount
            {
                UserId = userId,
                ExternalAccountId = externalAccountId,
                Type = channel,
            };
            setProperties?.Invoke(externalAccount);
            await db.ExternalAccounts.AddAsync(externalAccount);
            await db.SaveChangesAsync();
            return externalAccount;
        }
    }

    #region 创建用户

    /// <inheritdoc/>
    public async Task<(User user, IdentityResult identityResult)> CreateByPhoneNumberAsync(
        string phoneNumber,
        string? regionCode,
        bool phoneNumberConfirmed,
        string? password = null)
    {
        if (string.IsNullOrWhiteSpace(regionCode))
        {
            regionCode = IPhoneNumber.DefaultPhoneNumberRegionCode;
        }
        var user = new User
        {
            PhoneNumber = phoneNumber,
            PhoneNumberRegionCode = regionCode,
            PhoneNumberConfirmed = phoneNumberConfirmed,
        };
        var identityResult = await (string.IsNullOrWhiteSpace(password) ? CreateAsync(user) : CreateAsync(user, password));
        return (user, identityResult);
    }

    /// <inheritdoc/>
    public async Task<(User user, IdentityResult identityResult)> CreateByEmailAsync(
        string email,
        string password,
        bool emailConfirmed)
    {
        var user = new User
        {
            Email = email,
            EmailConfirmed = emailConfirmed,
        };
        var identityResult = await CreateAsync(user, password);
        return (user, identityResult);
    }

    void OnCreate(User user)
    {
        if (string.IsNullOrWhiteSpace(user.NickName))
        {
            user.NickName = user.GetNickName();
        }
    }

    /// <inheritdoc/>
    public override Task<IdentityResult> CreateAsync(User user)
    {
        OnCreate(user);
        return base.CreateAsync(user);
    }

    /// <inheritdoc/>
    public override Task<IdentityResult> CreateAsync(User user, string password)
    {
        OnCreate(user);
        return base.CreateAsync(user, password);
    }

    #endregion
}