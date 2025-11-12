using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Primitives.Models;
using Microsoft.AspNetCore.Identity;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;

/// <inheritdoc cref="UserManager{TUser}"/>
public partial interface IIdentityUserManager<TUser> : IDisposable where TUser : class
{
    /// <inheritdoc cref="UserManager{TUser}"/>
    UserManager<TUser> Impl { get; }

    /// <inheritdoc cref="UserManager{TUser}.IsLockedOutAsync(TUser)"/>
    Task<bool> IsLockedOutAsync(TUser user);

    /// <inheritdoc cref="UserManager{TUser}.SetPhoneNumberAsync(TUser, string?)"/>
    Task<IdentityResult> SetPhoneNumberAsync(TUser user, string? phoneNumber);

    /// <inheritdoc cref="UserManager{TUser}.SetPhoneNumberAsync(TUser, string?)"/>
    Task<IdentityResult> SetPhoneNumberAsync(TUser user, string? phoneNumber, string? phoneNumberRegionCode);

    /// <inheritdoc cref="UserManager{TUser}.HasPasswordAsync(TUser)"/>
    Task<bool> HasPasswordAsync(TUser user);

    /// <inheritdoc cref="UserManager{TUser}.AddPasswordAsync(TUser, string)"/>
    Task<IdentityResult> AddPasswordAsync(TUser user, string password);

    /// <inheritdoc cref="UserManager{TUser}.ChangePasswordAsync(TUser, string, string)"/>
    Task<IdentityResult> ChangePasswordAsync(TUser user, string currentPassword, string newPassword);

    /// <inheritdoc cref="UserManager{TUser}.GeneratePasswordResetTokenAsync(TUser)"/>
    Task<string> GeneratePasswordResetTokenAsync(TUser user);

    /// <inheritdoc cref="UserManager{TUser}.ResetPasswordAsync(TUser, string, string)"/>
    Task<IdentityResult> ResetPasswordAsync(TUser user, string token, string newPassword);

    /// <inheritdoc cref="UserManager{TUser}.FindByEmailAsync(string)"/>
    Task<TUser?> FindByEmailAsync(string email);

    /// <inheritdoc cref="UserManager{TUser}.CheckPasswordAsync(TUser, string)"/>
    Task<bool> CheckPasswordAsync(TUser user, string password);

    /// <inheritdoc cref="UserManager{TUser}.FindByIdAsync(string)"/>
    Task<TUser?> FindByIdAsync(Guid id);

    /// <inheritdoc cref="UserManager{TUser}.GenerateChangePhoneNumberTokenAsync(TUser, string)"/>
    Task<string> GenerateChangePhoneNumberTokenAsync(TUser user, string phoneNumber);

    /// <inheritdoc cref="UserManager{TUser}.ChangePhoneNumberAsync(TUser, string, string)"/>
    Task<IdentityResult> ChangePhoneNumberAsync(TUser user, string phoneNumber, string token);

    Task<IdentityResult> UpdateUserAsync(TUser user);
}

partial interface IIdentityUserManager<TUser> // 自定义方法
{
    /// <summary>
    /// 根据手机号查找用户
    /// </summary>
    Task<TUser?> FindByPhoneNumberAsync(string phoneNumber, string? regionCode);

    /// <summary>
    /// 根据 JWT Id 获取用户实体
    /// </summary>
    Task<TUser?> FindByTokenIdAsync(Guid jwtId);

    /// <summary>
    /// 根据 RefreshToken 刷新 Token 与新的 JwtId
    /// </summary>
    Task<JsonWebTokenValue?> RefreshTokenAsync(
        DevicePlatform2 platform,
        string? deviceId,
        string refresh_token);

    /// <summary>
    /// 通过多个条件查找用户。用户名/邮箱/手机号
    /// </summary>
    Task<TUser?> FindByAccountAsync(string account);

    /// <summary>
    /// 获取当前登录用户
    /// </summary>
    /// <returns></returns>
    Task<TUser?> GetUserAsync();

    /// <summary>
    /// 检查邮箱是否已注册
    /// </summary>
    Task<bool> ExistsEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// 刷新缓存的用户信息
    /// </summary>
    Task RefreshUserInfoCacheAsync(TUser user);

    /// <inheritdoc cref="RefreshUserInfoCacheAsync(TUser)"/>
    Task RefreshUserInfoCacheAsync(UserInfoModel userInfo);
}