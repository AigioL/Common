using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Response;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;

/// <summary>
/// 用户管理服务接口
/// </summary>
public partial interface IUserManager2 : IIdentityUserManager<User>
{
    /// <summary>
    /// 根据用户 Id 获取当前用户类型
    /// </summary>
    Task<UserType> GetUserTypeByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    //Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户信息，优先从缓存中取
    /// </summary>
    Task<UserInfoModel?> GetUserInfoCacheAsync();

    /// <summary>
    /// 进行登录生成凭证返回
    /// </summary>
    Task<ApiRsp<LoginOrRegisterResponse?>> LoginSharedAsync(
        User user,
        bool isLoginOrRegister,
        string? deviceId);

    /// <summary>
    /// 解绑第三方外部账号
    /// </summary>
    Task UnbundleAccountAsync(User user, ExternalLoginChannel channel);

    /// <summary>
    /// 登录或注册或绑定通过第三方外部账号
    /// </summary>
    Task<ApiRsp<LoginOrRegisterResponse?>> LoginOrRegisterOrBindAsync(
        string externalAccountId,
        ExternalLoginChannel channel,
        string deviceId,
        Guid? bindUserId = null,
        Action<ExternalAccount>? setProperties = null);
}

partial interface IUserManager2 // 创建用户
{
    /// <summary>
    /// 根据手机号码创建用户
    /// </summary>
    Task<(User user, IdentityResult identityResult)> CreateByPhoneNumberAsync(
        string phoneNumber,
        string? regionCode,
        bool phoneNumberConfirmed,
        string? password = null);

    /// <summary>
    /// 根据邮箱创建用户
    /// </summary>
    Task<(User user, IdentityResult identityResult)> CreateByEmailAsync(
        string email,
        string password,
        bool emailConfirmed);
}