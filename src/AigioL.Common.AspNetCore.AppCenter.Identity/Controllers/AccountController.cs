using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Response;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.SmsSender.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using R = AigioL.Common.AspNetCore.AppCenter.Identity.UI.Properties.Resources;
using RModelValid = AigioL.Common.AspNetCore.AppCenter.Properties.ModelValidationErrors;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

/// <summary>
/// 无登录状态匿名的账号相关终结点
/// </summary>
public static partial class AccountController
{
    internal const int MaxIpAccessFailedCount = 12;

    internal static string GetIpCacheKey(string ip) => $"AC_MS_LoginOrRegister_Ip_[{ip}]";

    internal static TimeSpan GetLockoutEnd() => TimeSpan.FromMinutes(7);

    public static void MapIdentityAccountV5<TIdentityDbContext>(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v5/account")
        where TIdentityDbContext : IIdentityDbContext
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapPost("loginorregister", async (HttpContext context,
            [FromBody] LoginOrRegisterRequest request) =>
        {
            var deviceId = request.GetDeviceId();
            var r = await LoginOrRegister<LoginOrRegisterResponse, TIdentityDbContext>(context,
                request.PhoneNumber,
                request.PhoneNumberRegionCode,
                request.SmsCode,
                request.Channel,
                deviceId,
                (userManager, user, isLoginOrRegister) => userManager.LoginSharedAsync(user, isLoginOrRegister, deviceId));
            return r;
        }).WithDescription("登录或注册账号")
        .WithRequiredSecurityKey();
        routeGroup.MapPost("refreshtoken", async (HttpContext context,
            [FromBody] RefreshTokenRequest request) =>
        {
            var deviceId = request.GetDeviceId();
            var r = await RefreshTokenAsync(context, request.RefreshToken, deviceId);
            return r;
        }).WithDescription("刷新 JWT")
        .WithRequiredSecurityKey();
        routeGroup.MapPost("validateRegisterEmail", async (HttpContext context,
            [FromBody] ValidateRegisterEmailRequest request) =>
        {
            var r = await ValidateRegisterEmail(context, request.Email);
            return r;
        }).WithDescription("验证注册邮箱账号")
        .WithRequiredSecurityKey();
        routeGroup.MapPost("resetPassword", async (HttpContext context,
            [FromBody] ResetPasswordRequest request) =>
        {
            var authMessageRecordRepo = context.RequestServices.GetRequiredService<IAuthMessageRecordRepository>();
            var userManager = context.RequestServices.GetRequiredService<IUserManager2>();
            var smsSender = context.RequestServices.GetRequiredService<ISmsSender>();
            var r = await ResetPassword(context, authMessageRecordRepo, userManager,
                smsSender, request.Type, request.PhoneNumber,
                request.PhoneNumberRegionCode, request.Email, request.OTPCode,
                request.Password, request.Password2);
            return r;
        }).WithDescription("重置密码")
        .WithRequiredSecurityKey();
        routeGroup.MapPost("registerByEmail", async (HttpContext context,
            [FromBody] RegisterByEmailRequest request) =>
        {
            var deviceId = request.GetDeviceId();
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(AccountController));
            var userManager = context.RequestServices.GetRequiredService<IUserManager2>();
            var db = context.RequestServices.GetRequiredService<TIdentityDbContext>();
            var authMessageRecordRepo = context.RequestServices.GetRequiredService<IAuthMessageRecordRepository>();
            var smsSender = context.RequestServices.GetRequiredService<ISmsSender>();
            var r = await RegisterByEmail(logger,
                userManager,
                db,
                authMessageRecordRepo,
                smsSender,
                request.Email,
                request.Password,
                request.Code,
                deviceId,
                (userManager, user, isLoginOrRegister) => userManager.LoginSharedAsync(user, isLoginOrRegister, deviceId));
            return r;
        }).WithDescription("邮箱注册账号")
        .WithRequiredSecurityKey();
        routeGroup.MapPost("loginByPassword", async (HttpContext context,
            [FromBody] AccountLoginRequest request) =>
        {
            var deviceId = request.GetDeviceId();
            var r = await LoginByPassword<LoginOrRegisterResponse, TIdentityDbContext>(
                context,
                request.Account,
                request.Password,
                (userManager, user, isLoginOrRegister) => userManager.LoginSharedAsync(user, isLoginOrRegister, deviceId));
            return r;
        }).WithDescription("密码登录账号")
        .WithRequiredSecurityKey();
#if DEBUG
        routeGroup.MapGet("test/ex", async (HttpContext context) =>
        {
            await Task.Delay(10, context.RequestAborted);
            throw new ApplicationException("测试 WithRequiredSecurityKey 的异常页面");
        }).WithIdentityUIView();
#endif
    }

    /// <summary>
    /// 登录或注册账号，如要支持密码登录，需使用 <see cref="SignInManager{TUser}"/> 提供纪录失败次数与锁定用户
    /// </summary>
    static async Task<ApiRsp<TLoginOrRegisterResponse?>> LoginOrRegister<TLoginOrRegisterResponse, TIdentityDbContext>(
        HttpContext context,
        string? phoneNumber,
        string? phoneNumberRegionCode,
        string? smsCode,
        LoginChannel loginChannel,
        string? deviceId,
        Func<IUserManager2, User, bool, Task<ApiRsp<TLoginOrRegisterResponse?>>> funcLoginSharedAsync)
        where TIdentityDbContext : IIdentityDbContext
    {
        if (string.IsNullOrWhiteSpace(phoneNumberRegionCode))
        {
            phoneNumberRegionCode = IPhoneNumber.DefaultPhoneNumberRegionCode;
        }

        if (!context.GetRemoteIpAddress(out var ip))
        {
            return R.未知的IP地址;
        }

        var ipCacheKey = GetIpCacheKey(ip);
        var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
        var ipAccessFailedCount = await cache.GetV2Async<int>(ipCacheKey, context.RequestAborted);
        if (ipAccessFailedCount >= MaxIpAccessFailedCount)
        {
            return HttpStatusCode.TooManyRequests;
        }

        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(AccountController));
        var userManager = context.RequestServices.GetRequiredService<IUserManager2>();
        var db = context.RequestServices.GetRequiredService<TIdentityDbContext>();
        var authMessageRecordRepo = context.RequestServices.GetRequiredService<IAuthMessageRecordRepository>();
        var smsSender = context.RequestServices.GetRequiredService<ISmsSender>();

        var result = await LoginOrRegisterCore(
            logger, userManager, db,
            authMessageRecordRepo, smsSender, phoneNumber,
            phoneNumberRegionCode, smsCode, funcLoginSharedAsync,
            context.RequestAborted);
        if (result.IsSuccess())
        {
            cache.Remove(ipCacheKey);
        }
        else
        {
            var lockoutEnd = GetLockoutEnd();
            await cache.SetV2Async(ipCacheKey, ipAccessFailedCount + 1,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = lockoutEnd,
                });
        }
        return result;
    }

    static async Task<ApiRsp<TLoginOrRegisterResponse?>> LoginOrRegisterCore<TLoginOrRegisterResponse>(
        ILogger logger,
        IUserManager2 userManager,
        IIdentityDbContext db,
        IAuthMessageRecordRepository authMessageRecordRepo,
        ISmsSender smsSender,
        string? phoneNumber,
        string? phoneNumberRegionCode,
        string? smsCode,
        Func<IUserManager2, User, bool, Task<ApiRsp<TLoginOrRegisterResponse?>>> funcLoginSharedAsync,
        CancellationToken cancellationToken = default)
    {
        if (phoneNumber == null)
        {
            return ApiRspCode.RequestModelValidateFail;
        }
        if (smsCode == null)
        {
            return ApiRspCode.RequestModelValidateFail;
        }

        var user = await userManager.FindByPhoneNumberAsync(phoneNumber, phoneNumberRegionCode);

        // https://docs.microsoft.com/zh-cn/ef/core/saving/transactions
        using var transaction = await db.GetDatabase().BeginTransactionAsync(cancellationToken);

        var smsType = user == null ? SmsCodeType.Register : SmsCodeType.Login;
        var record = await authMessageRecordRepo.CheckAuthMessageAsync(
               smsSender,
               phoneNumber,
               phoneNumberRegionCode,
               smsCode,
               smsType); // 校验短信验证码

        if (record == null || record.Abandoned)
        {
            await transaction.CommitAsync(cancellationToken);
            return R.验证码已过期或不存在;
        }
        if (!record.CheckSuccess)
        {
            await transaction.CommitAsync(cancellationToken);
            return R.短信验证码不正确;
        }


        if (user == null) // Register
        {
            // 占位符 - 邀请码(InviteCode)
            try
            {
                (var user_c, var result) =
                    await userManager.CreateByPhoneNumberAsync(
                        phoneNumber,
                        phoneNumberRegionCode,
                        phoneNumberConfirmed: true);

                user = user_c;

                if (result.Succeeded)
                {
                    return await LoginSharedAsync(false);
                }
                return result.Fail<TLoginOrRegisterResponse?>();
            }
            catch (Exception ex)
            {
#pragma warning disable CA1873 // Avoid potentially expensive logging
                LogErrorOnRegister(logger, ex,
                    IPhoneNumber.ToStringHideMiddleFour(phoneNumber),
                    phoneNumberRegionCode);
#pragma warning restore CA1873 // Avoid potentially expensive logging
                return ApiRspCode.InternalServerError;
            }
        }
        else
        {
            if (user.Id == default) // 默认值 Id 保留给客服账号
            {
                await transaction.CommitAsync(cancellationToken);
                return R.无法登录客服账户;
            }
            return await LoginSharedAsync(true);
        }

        async Task<ApiRsp<TLoginOrRegisterResponse?>> LoginSharedAsync(bool isLoginOrRegister)
        {
            var r = await funcLoginSharedAsync(userManager, user, isLoginOrRegister);
            if (r.IsSuccess())
            {
                await transaction.CommitAsync(cancellationToken);
            }
            return r;
        }
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "注册用户失败，手机号：{phoneNumberRegionCode}-{phoneNumber}")]
    private static partial void LogErrorOnRegister(
        ILogger logger, Exception ex, string? phoneNumber, string? phoneNumberRegionCode);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "注册用户失败，邮箱：{email}")]
    private static partial void LogErrorOnRegisterByEmail(
        ILogger logger, Exception ex, string? email);

    /// <summary>
    /// 刷新 JWT
    /// </summary>
    static async Task<ApiRsp<JsonWebTokenValue?>> RefreshTokenAsync(
        HttpContext context,
        string? refresh_token,
        string? deviceId)
    {
        if (string.IsNullOrWhiteSpace(refresh_token))
        {
            return HttpStatusCode.Unauthorized;
        }

        var platform = context.GetDevicePlatform();
        var userManager = context.RequestServices.GetRequiredService<IUserManager2>();
        var newToken = await userManager.RefreshTokenAsync(platform, deviceId, refresh_token);
        if (newToken == null)
        {
            return HttpStatusCode.Unauthorized;
        }
        return newToken;
    }

    /// <summary>
    /// 验证注册邮箱
    /// </summary>
    static async Task<ApiRsp> ValidateRegisterEmail(
        HttpContext context,
        string email)
    {
        // 参数校验
        if (!email.IsEmail())
        {
            return HttpStatusCode.BadRequest;
        }

        if (!context.GetRemoteIpAddress(out var ip))
        {
            return R.未知的IP地址;
        }

        var ipCacheKey = GetIpCacheKey(ip);
        var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
        var ipAccessFailedCount = await cache.GetV2Async<int>(ipCacheKey, context.RequestAborted);
        if (ipAccessFailedCount >= MaxIpAccessFailedCount)
        {
            return HttpStatusCode.TooManyRequests;
        }

        var userManager = context.RequestServices.GetRequiredService<IUserManager2>();
        var exists = await userManager.ExistsEmailAsync(email, context.RequestAborted);
        if (exists)
        {
            return R.该邮箱已被其它用户使用;
        }
        return HttpStatusCode.OK;
    }

    static async Task<string?> CheckAuthMessageAsync(
        IAuthMessageRecordRepository authMessageRecordRepo,
        ISmsSender smsSender,
        string phoneNumberOrEmail,
        string? phoneNumberRegionCode,
        string smsCode,
        SmsCodeType smsCodeType,
        AuthMessageType type)
    {
        var record = await authMessageRecordRepo.CheckAuthMessageAsync(
            smsSender, phoneNumberOrEmail, phoneNumberRegionCode,
            smsCode, smsCodeType, type);
        if (record == null || record.Abandoned)
        {
            return R.验证码已过期或不存在;
        }
        if (!record.CheckSuccess)
        {
            return R.短信验证码不正确;
        }
        return null;
    }

    /// <summary>
    /// 重置密码
    /// </summary>
    static async Task<ApiRsp> ResetPassword(
        HttpContext context,
        IAuthMessageRecordRepository authMessageRecordRepo,
        IUserManager2 userManager,
        ISmsSender smsSender,
        AuthMessageType type,
        string? phoneNumber,
        string? phoneNumberRegionCode,
        string? email,
        string? smsCode,
        string? pwd,
        string? pwd2)
    {
        // 参数校验
        if (string.IsNullOrWhiteSpace(smsCode) || string.IsNullOrWhiteSpace(pwd) || pwd != pwd2)
        {
            return HttpStatusCode.BadRequest;
        }

        if (string.IsNullOrWhiteSpace(phoneNumberRegionCode))
        {
            phoneNumberRegionCode = IPhoneNumber.DefaultPhoneNumberRegionCode;
        }

        if (!context.GetRemoteIpAddress(out var ip))
        {
            return R.未知的IP地址;
        }

        var ipCacheKey = GetIpCacheKey(ip);
        var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
        var ipAccessFailedCount = await cache.GetV2Async<int>(ipCacheKey, context.RequestAborted);
        if (ipAccessFailedCount >= MaxIpAccessFailedCount)
        {
            return HttpStatusCode.TooManyRequests;
        }

        const SmsCodeType smsCodeType = SmsCodeType.ResetPassword;
        User? user;
        switch (type)
        {
            case AuthMessageType.PhoneNumber:
                {
                    if (string.IsNullOrWhiteSpace(phoneNumber))
                    {
                        return RModelValid.请填写手机号码;
                    }
                    var error = await CheckAuthMessageAsync(
                        authMessageRecordRepo, smsSender, phoneNumber,
                        phoneNumberRegionCode, smsCode, smsCodeType,
                        type);
                    if (error != null)
                    {
                        return error;
                    }
                    user = await userManager.FindByPhoneNumberAsync(phoneNumber, phoneNumberRegionCode);
                }
                break;
            case AuthMessageType.Email:
                {
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        return RModelValid.请填写邮箱;
                    }
                    if (!email.IsEmail())
                    {
                        return HttpStatusCode.BadRequest;
                    }
                    var error = await CheckAuthMessageAsync(
                        authMessageRecordRepo, smsSender, email,
                        null, smsCode, smsCodeType,
                        type);
                    if (error != null)
                    {
                        return error;
                    }
                    user = await userManager.FindByEmailAsync(email);
                }
                break;
            default:
                return R.验证方式错误;
        }

        if (user == null)
        {
            return R.用户不存在;
        }
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, pwd);
        if (result.Succeeded)
        {
            await userManager.RefreshUserInfoCacheAsync(user);
            return HttpStatusCode.OK;
        }

        return result.Fail();
    }

    /// <summary>
    /// 通过邮箱注册
    /// </summary>
    static async Task<ApiRsp<TLoginOrRegisterResponse?>> RegisterByEmail<TLoginOrRegisterResponse>(
        ILogger logger,
        IUserManager2 userManager,
        IIdentityDbContext db,
        IAuthMessageRecordRepository authMessageRecordRepo,
        ISmsSender smsSender,
        string? email,
        string? password,
        string? code,
        string? deviceId,
        Func<IUserManager2, User, bool, Task<ApiRsp<TLoginOrRegisterResponse?>>> funcLoginSharedAsync,
        CancellationToken cancellationToken = default) where TLoginOrRegisterResponse : class
    {
        if (string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(code) ||
            string.IsNullOrEmpty(deviceId))
        {
            return ApiRspCode.RequestModelValidateFail;
        }

        var exists = await userManager.ExistsEmailAsync(email, cancellationToken);
        if (exists)
        {
            return R.该邮箱已被其它用户使用;
        }

        // https://docs.microsoft.com/zh-cn/ef/core/saving/transactions
        using var transaction = await db.GetDatabase().BeginTransactionAsync(cancellationToken);

        var record = await authMessageRecordRepo.CheckAuthMessageAsync(
               smsSender,
               email,
               default,
               code,
               SmsCodeType.Register,
               AuthMessageType.Email); // 校验短信验证码

        if (record == null || record.Abandoned)
        {
            await transaction.CommitAsync(cancellationToken);
            return R.验证码已过期或不存在;
        }
        if (!record.CheckSuccess)
        {
            await transaction.CommitAsync(cancellationToken);
            return R.短信验证码不正确;
        }

        try
        {
            (var user, var result) = await userManager.CreateByEmailAsync(email, password, emailConfirmed: true);
            if (result.Succeeded)
            {
                var r = await funcLoginSharedAsync(userManager, user, false);
                if (r.IsSuccess())
                    await transaction.CommitAsync(cancellationToken);
                return r;
            }

            return result.Fail<TLoginOrRegisterResponse?>();
        }
        catch (Exception ex)
        {
            LogErrorOnRegisterByEmail(logger, ex,
                email);
            return ApiRspCode.InternalServerError;
        }
    }

    /// <summary>
    /// 通过密码登录
    /// </summary>
    static async Task<ApiRsp<TLoginOrRegisterResponse?>> LoginByPassword<TLoginOrRegisterResponse, TIdentityDbContext>(
        HttpContext context,
        string? account,
        string? password,
        Func<IUserManager2, User, bool, Task<ApiRsp<TLoginOrRegisterResponse?>>> funcLoginSharedAsync)
        where TIdentityDbContext : IIdentityDbContext
    {
        if (!context.GetRemoteIpAddress(out var ip))
        {
            return R.未知的IP地址;
        }

        var ipCacheKey = GetIpCacheKey(ip);
        var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
        var ipAccessFailedCount = await cache.GetV2Async<int>(ipCacheKey, context.RequestAborted);
        if (ipAccessFailedCount >= MaxIpAccessFailedCount)
        {
            return HttpStatusCode.TooManyRequests;
        }

        var userManager = context.RequestServices.GetRequiredService<IUserManager2>();
        var db = context.RequestServices.GetRequiredService<TIdentityDbContext>();
        var result = await LoginByPasswordCore(
            userManager, db, account,
            password, funcLoginSharedAsync, context.RequestAborted);
        if (result.IsSuccess())
        {
            cache.Remove(ipCacheKey);
        }
        else
        {
            var lockoutEnd = GetLockoutEnd();
            await cache.SetV2Async(ipCacheKey, ipAccessFailedCount + 1,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = lockoutEnd,
                });
        }
        return result;
    }

    static async Task<ApiRsp<TLoginOrRegisterResponse?>> LoginByPasswordCore<TLoginOrRegisterResponse>(
        IUserManager2 userManager,
        IIdentityDbContext db,
        string? account,
        string? password,
        Func<IUserManager2, User, bool, Task<ApiRsp<TLoginOrRegisterResponse?>>> funcLoginSharedAsync,
        CancellationToken cancellationToken = default)
    {
        if (account == null)
        {
            return ApiRspCode.RequestModelValidateFail;
        }
        if (password == null)
        {
            return ApiRspCode.RequestModelValidateFail;
        }

        var user = await userManager.FindByAccountAsync(account);
        if (user == null)
        {
            return R.账号或密码错误;
        }
        var checkPassword = await userManager.CheckPasswordAsync(user, password);
        if (!checkPassword)
        {
            return R.账号或密码错误;
        }

        // https://docs.microsoft.com/zh-cn/ef/core/saving/transactions
        using var transaction = await db.GetDatabase().BeginTransactionAsync(cancellationToken);

        var result = await funcLoginSharedAsync(userManager, user, true);
        if (result.IsSuccess())
        {
            await transaction.CommitAsync(cancellationToken);
        }
        return result;
    }
}

//file static class B9490399
//{

//}