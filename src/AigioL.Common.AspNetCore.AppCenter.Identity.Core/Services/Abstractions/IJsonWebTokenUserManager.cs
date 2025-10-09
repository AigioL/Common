using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Response;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using Microsoft.AspNetCore.Identity;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;

public interface IJsonWebTokenUserManager
{
    /// <summary>
    /// 根据手机号查找用户
    /// </summary>
    Task<User?> FindByPhoneNumberAsync(
        string phoneNumber,
        string? regionCode);

    Task<User?> FindByEmailAsync(string email);

    /// <summary>
    /// 根据 RefreshToken 刷新 Token 与新的 JwtId
    /// </summary>
    Task<JsonWebTokenValue?> RefreshTokenAsync(
        DevicePlatform2 platform,
        string? deviceId,
        string refresh_token);

    /// <summary>
    /// 根据手机号码创建用户
    /// </summary>
    Task<(User user, IdentityResult identityResult)> CreateByPhoneNumberAsync(
        string phoneNumber,
        string? regionCode,
        bool phoneNumberConfirmed);

    /// <summary>
    /// 进行登录生成凭证返回（版本 0）
    /// </summary>
    Task<ApiRsp<LoginOrRegisterResponseV0?>> LoginSharedV0Async(
        User user,
        bool isLoginOrRegister,
        string? deviceId);

    /// <summary>
    /// 进行登录生成凭证返回（版本 -1）
    /// </summary>
    [Obsolete("use LoginSharedV0Async")]
    Task<ApiRsp<LoginOrRegisterResponseV_1?>> LoginSharedV_1Async(
        User user,
        bool isLoginOrRegister,
        string? deviceId);

    Task<bool> ExistsEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<string> GeneratePasswordResetTokenAsync(User user);

    Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);

    /// <summary>
    /// 刷新缓存的用户信息
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task RefreshUserInfoCacheAsync(User user);

    /// <summary>
    /// 根据邮箱创建用户
    /// </summary>
    Task<(User user, IdentityResult identityResult)> CreateByEmailAsync(string email, string password, bool emailConfirmed);

    /// <summary>
    /// 通过多个条件查找用户，（用户名/邮箱/手机号）
    /// </summary>
    /// <param name="account"></param>
    /// <returns></returns>
    Task<User?> FindByAccountAsync(string account);

    /// <summary>
    /// 检查用户密码是否正确
    /// </summary>
    /// <param name="user">用户</param>
    /// <param name="password">密码</param>
    /// <returns></returns>
    Task<bool> CheckPasswordAsync(User user, string password);
}
