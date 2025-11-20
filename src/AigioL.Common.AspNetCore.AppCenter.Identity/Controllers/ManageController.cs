using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;
using AigioL.Common.SmsSender.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using R = AigioL.Common.AspNetCore.AppCenter.Identity.UI.Properties.Resources;
using RModelValid = AigioL.Common.AspNetCore.AppCenter.Properties.ModelValidationErrors;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

/// <summary>
/// 登录后用户管理终结点
/// </summary>
public static partial class ManageController
{
    public static void MapIdentityManageV5<TIdentityDbContext>(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v5/manage")
        where TIdentityDbContext : IIdentityDbContext
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize);

        #region 刷新用户信息

        routeGroup.MapGet("refreshuserinfo", async (HttpContext context) =>
        {
            var r = await RefreshUserInfoAsync(context);
            return r;
        }).WithDescription("刷新用户信息")
        .WithRequiredSecurityKey();
        routeGroup.MapPost("refreshuserinfo", async (HttpContext context) =>
        {
            var r = await RefreshUserInfoAsync(context);
            return r;
        }).ExcludeFromDescription()
        .WithRequiredSecurityKey();

        #endregion

        #region 邮箱

        routeGroup.MapPost("sendbindemail", async (HttpContext context) =>
        {
            ApiRsp r = "TODO: 待完成";
            return r;
        }).WithDescription("发送绑定邮箱邮件")
        .WithRequiredSecurityKey();
        routeGroup.MapPost("bindemail", async (HttpContext context) =>
        {
            ApiRsp r = "TODO: 待完成";
            return r;
        }).WithDescription("绑定邮箱")
        .WithRequiredSecurityKey();
        routeGroup.MapPost("changebindemail", async (HttpContext context) =>
        {
            ApiRsp r = "TODO: 待完成";
            return r;
        }).WithDescription("换绑邮箱")
        .WithRequiredSecurityKey();

        #endregion

        #region 换绑手机（安全验证）/换绑手机（绑定新手机号）/绑定手机号（首次绑定）

        routeGroup.MapPost("changebindphonenumber", async (HttpContext context,
            [FromBody] ChangePhoneNumberValidationRequest request) =>
        {
            var smsSender = context.RequestServices.GetRequiredService<ISmsSender>();
            var userManager = context.RequestServices.GetRequiredService<IUserManager2>();
            var authMessageRecordRepo = context.RequestServices.GetRequiredService<IAuthMessageRecordRepository>();
            var db = context.RequestServices.GetRequiredService<TIdentityDbContext>();
            var r = await ChangeBindPhoneNumberCoreAsync(
                smsSender, userManager, authMessageRecordRepo,
                db, request, ResultOkString);
            return r;
        }).WithDescription("换绑手机（安全验证）")
        .WithRequiredSecurityKey();
        routeGroup.MapPut("changebindphonenumber", async (HttpContext context,
            [FromBody] ChangePhoneNumberNewRequest request) =>
        {
            var smsSender = context.RequestServices.GetRequiredService<ISmsSender>();
            var userManager = context.RequestServices.GetRequiredService<IUserManager2>();
            var authMessageRecordRepo = context.RequestServices.GetRequiredService<IAuthMessageRecordRepository>();
            var db = context.RequestServices.GetRequiredService<TIdentityDbContext>();
            var r = await ChangeBindPhoneNumberCoreAsync(
                smsSender, userManager, authMessageRecordRepo,
                db, request);
            return r;
        }).WithDescription("换绑手机（绑定新手机号）")
        .WithRequiredSecurityKey();
        routeGroup.MapPost("bindphonenumber", async (HttpContext context,
            [FromBody] BindPhoneNumberRequest request) =>
        {
            var r = await RefreshUserInfoAsync(context);
            return r;
        }).WithDescription("绑定手机号（首次绑定）")
        .WithRequiredSecurityKey();

        #endregion

        #region 签到

        routeGroup.MapPost("clockin", async (HttpContext context) =>
        {
            ApiRsp r = "TODO: 待完成";
            return r;
        }).WithDescription("每日签到")
        .WithRequiredSecurityKey();
        routeGroup.MapGet("clockinrecords", async (HttpContext context) =>
        {
            ApiRsp r = "TODO: 待完成";
            return r;
        }).WithDescription("获取每日签到记录")
        .WithRequiredSecurityKey();

        #endregion

        routeGroup.MapDelete("deleteaccount", async (HttpContext context) =>
        {
            var r = await DeleteAccountCoreAsync<TIdentityDbContext>(context);
            return r;
        }).WithDescription("注销（删除）账号")
        .WithRequiredSecurityKey();
        routeGroup.MapPost("setPassword", async (HttpContext context,
            [FromBody] SetPasswordRequest request) =>
        {
            ApiRsp r = "TODO: 待完成";
            return r;
        }).WithDescription("设置账号密码")
        .WithRequiredSecurityKey();
        routeGroup.MapPost("edituserprofile", async (HttpContext context,
            [FromBody] EditUserProfileRequest request) =>
        {
            var userManager = context.RequestServices.GetRequiredService<IUserManager2>();
            var r = await EditUserProfileCoreAsync(userManager, request);
            return r;
        }).WithDescription("编辑个人资料")
        .WithRequiredSecurityKey();
        routeGroup.MapGet("signout", async (HttpContext context) =>
        {
            var r = await SignOutCoreAsync<TIdentityDbContext>(context);
            return r;
        }).WithDescription("退出登录（登出）账号")
        .WithRequiredSecurityKey();
        routeGroup.MapDelete("unbundleaccount/{channel}", async (HttpContext context,
            [FromRoute] string channel) =>
        {
            ApiRsp r;
            if (Enum.TryParse<ExternalLoginChannel>(channel, true, out var channelE))
            {
                var userManager = context.RequestServices.GetRequiredService<IUserManager2>();
                r = await UnbundleAccountCoreAsync(userManager, channelE);
            }
            else
            {
                r = ApiRspCode.NotFound;
            }
            return r;
        }).WithDescription("解绑账号的第三方外部平台，例如 Steam、WeChat、QQ、Alipay 等")
        .WithRequiredSecurityKey();
    }

    static async Task<ApiRsp<UserInfoModel?>> RefreshUserInfoAsync(
        HttpContext context)
    {
        var userId = context.GetUserId();
        if (userId == null)
        {
            return ApiRspCode.Unauthorized;
        }

        var userManager = context.RequestServices.GetRequiredService<IUserManager2>();
        var userInfo = await userManager.GetUserInfoCacheAsync();
        return userInfo;
    }

    static async Task<ApiRsp> DeleteAccountCoreAsync<TIdentityDbContext>(
        HttpContext context)
        where TIdentityDbContext : IIdentityDbContext
    {
        var userId = context.GetUserId();
        if (!userId.HasValue)
        {
            return ApiRspCode.Unauthorized;
        }

        var jwtUserId = context.GetJwtUserId();
        if (!jwtUserId.HasValue)
        {
            return ApiRspCode.Unauthorized;
        }

        var db = context.RequestServices.GetRequiredService<TIdentityDbContext>();
        var userDeleteRepo = context.RequestServices.GetRequiredService<IUserDeleteRepository>();
        var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
        var connection = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        var r = await DeleteAccountCoreAsync(
            userId.Value, jwtUserId.Value, db,
            userDeleteRepo, cache, connection);
        return r;
    }

    /// <summary>
    /// 删除账号
    /// </summary>
    static async Task<ApiRsp> DeleteAccountCoreAsync(
        Guid userId,
        Guid jwtUserId,
        IIdentityDbContext db,
        IUserDeleteRepository userDeleteRepo,
        IDistributedCache cache,
        IConnectionMultiplexer connection)
    {
        await SignOutSharedAsync(jwtUserId, db, cache, connection);
        var redisDb = connection.GetDatabase(CacheKeys.RedisHashDataDb);

        var query = from m in db.ExternalAccounts
                    where m.UserId == userId
                    select new KeyValuePair<ExternalLoginChannel, string>(m.Type, m.ExternalAccountId);
        var externalAccounts = await query.ToArrayAsync();

        foreach (var it in externalAccounts)
        {
            var hashKey = $"{CacheKeys.IdentityUserExternalAccountsHashKey}_C_{it.Key}";
            await redisDb.HashDeleteAsync(hashKey, it.Value);
        }
        await userDeleteRepo.DeleteAccountAsync(userId);

        return true;
    }

    static ApiRsp<string?> ResultOkString(string? val) => ApiRsp.Ok(val);

    /// <summary>
    /// 换绑手机（安全验证）
    /// </summary>
    static async Task<ApiRsp<T?>> ChangeBindPhoneNumberCoreAsync<T>(
        ISmsSender smsSender,
        IUserManager2 userManager,
        IAuthMessageRecordRepository authMessageRecordRepo,
        IIdentityDbContext db,
        ChangePhoneNumberValidationRequest request,
        Func<string, ApiRsp<T?>> resultOk)
    {
        if (request.PhoneNumber == null)
        {
            return ApiRspCode.RequestModelValidateFail;
        }
        if (request.SmsCode == null)
        {
            return ApiRspCode.RequestModelValidateFail;
        }

        if (request.PhoneNumber == IPhoneNumber.SimulatorDefaultValue)
        {
            return RModelValid.请输入正确的手机号码哦;
        }

        var findUser = await userManager.FindByPhoneNumberAsync(request.PhoneNumber, request.PhoneNumberRegionCode);
        if (findUser != null)
        {
            return R.手机号码已存在_换绑手机;
        }

        var user = await userManager.GetUserAsync();
        if (user == null)
        {
            return ApiRspCode.Unauthorized;
        }

        if (string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            return R.当前手机号码不存在;
        }

        if (request.PhoneNumber == user.PhoneNumber)
        {
            return R.新手机号不能与旧手机号一样;
        }

        // https://docs.microsoft.com/zh-cn/ef/core/saving/transactions
        using var transaction = await db.GetDatabase().BeginTransactionAsync();

        var record = await authMessageRecordRepo.CheckAuthMessageAsync(
            smsSender,
            user.PhoneNumber,
            user.PhoneNumberRegionCode,
            request.SmsCode,
            SmsCodeType.ChangePhoneNumberValidation);

        if (record == null || record.Abandoned)
        {
            await transaction.CommitAsync();
            return R.验证码已过期或不存在;
        }

        if (!record.CheckSuccess)
        {
            await transaction.CommitAsync();
            return R.验证码不正确;
        }

        var code = await userManager.GenerateChangePhoneNumberTokenAsync(user, request.PhoneNumber);
        await transaction.CommitAsync();

        var r = resultOk(code);
        return r;
    }

    /// <summary>
    /// 绑定新手机号
    /// </summary>
    static async Task<ApiRsp> ChangeBindPhoneNumberCoreAsync(
        ISmsSender smsSender,
        IUserManager2 userManager,
        IAuthMessageRecordRepository authMessageRecordRepo,
        IIdentityDbContext db,
        ChangePhoneNumberNewRequest request)
    {
        if (request.PhoneNumber == null)
        {
            return ApiRspCode.RequestModelValidateFail;
        }
        if (request.SmsCode == null)
        {
            return ApiRspCode.RequestModelValidateFail;
        }

        if (request.PhoneNumber == IPhoneNumber.SimulatorDefaultValue)
        {
            return RModelValid.请输入正确的手机号码哦;
        }

        if (request.Code == null)
        {
            // 没有通过当前手机号验证
            return "Code is invalid.";
        }

        var user = await userManager.GetUserAsync();
        if (user == null) return ApiRspCode.Unauthorized;

        if (request.PhoneNumber == user.PhoneNumber)
        {
            // 新手机号不能与旧手机号一样
            return "new phone number cannot be the same as the old phone number.";
        }

        // https://docs.microsoft.com/zh-cn/ef/core/saving/transactions
        using var transaction = await db.GetDatabase().BeginTransactionAsync();

        var record = await authMessageRecordRepo.CheckAuthMessageAsync(
            smsSender,
            request.PhoneNumber,
            request.PhoneNumberRegionCode,
            request.SmsCode,
            SmsCodeType.ChangePhoneNumberNew);

        if (record == null || record.Abandoned)
        {
            await transaction.CommitAsync();
            return R.验证码已过期或不存在;
        }

        if (!record.CheckSuccess)
        {
            await transaction.CommitAsync();
            return R.验证码不正确;
        }

        var result = await userManager.ChangePhoneNumberAsync(user, request.PhoneNumber, request.Code);
        if (result.Succeeded)
        {
            await transaction.CommitAsync();
            var userInfo = await userManager.GetUserInfoCacheAsync();
            if (userInfo is not null)
            {
                userInfo.PhoneNumber = user.PhoneNumber;
                await userManager.RefreshUserInfoCacheAsync(userInfo);
            }
            return true;
        }

        return result.Fail();
    }

    /// <summary>
    /// 解绑第三方平台关联账号
    /// </summary>
    static async Task<ApiRsp> UnbundleAccountCoreAsync(
        IUserManager2 userManager,
        ExternalLoginChannel channel)
    {
        if (!Enum.IsDefined(channel))
        {
            return ApiRspCode.BadRequest;
        }

        var user = await userManager.GetUserAsync();
        if (user == null)
        {
            return ApiRspCode.Unauthorized;
        }

        if (string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            return ApiRspCode.BadRequest;
        }

        await userManager.UnbundleAccountAsync(user, channel);
        await userManager.RefreshUserInfoCacheAsync(user);

        return true;
    }

    /// <summary>
    /// 登出，退出登录
    /// </summary>
    static async Task<ApiRsp> SignOutCoreAsync<TIdentityDbContext>(
        HttpContext context)
        where TIdentityDbContext : IIdentityDbContext
    {
        var jwtUserId = context.GetJwtUserId();
        if (jwtUserId.HasValue)
        {
            var db = context.RequestServices.GetRequiredService<TIdentityDbContext>();
            var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
            var connection = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
            await SignOutSharedAsync(jwtUserId.Value, db, cache, connection);
        }
        return true;
    }

    static async Task SignOutSharedAsync(
        Guid jwtUserId,
        IIdentityDbContext db,
        IDistributedCache cache,
        IConnectionMultiplexer connection)
    {
        var jwtUserIdS = ShortGuid.Encode(jwtUserId);
        await db.UserJsonWebTokens.Where(x => x.Id == jwtUserId).ExecuteDeleteAsync();
        var redisdb = connection.GetDatabase(CacheKeys.RedisHashDataDb);
        await redisdb.HashDeleteAsync(CacheKeys.IdentityUserInfoDataHashV1Key, jwtUserIdS);
        await redisdb.HashDeleteAsync(CacheKeys.IdentityUserDeviceIsTrustWithUserIdMapHashKey, jwtUserIdS);
        await cache.RemoveAsync(jwtUserIdS);
        await db.UserRefreshJsonWebTokens.Where(x => x.Id == jwtUserId).ExecuteDeleteAsync();
    }

    ///// <summary>
    ///// 获取每日签到记录，用于 UI 上显示日历并且标记 ✅
    ///// </summary>
    //static async Task<ApiRsp<DateTimeOffset[]?>> ClockInLogsCoreAsync(
    //    IUserManager2 userManager,
    //    IClockInRecordRepository clockInRecordRepo,
    //    DateTimeOffset? time)
    //{
    //    var user = await userManager.GetUserAsync();
    //    if (user == null)
    //        return ApiRspCode.Unauthorized;
    //    var result = await clockInRecordRepo.GetClockOfMonthAsync(user.Id, time ?? DateTimeOffset.Now);
    //    return result;
    //}

    /// <summary>
    /// 编辑个人资料
    /// </summary>
    static async Task<ApiRsp> EditUserProfileCoreAsync(
        IUserManager2 userManager,
        EditUserProfileRequest request)
    {
        //var errorMessage = request.GetErrorMessage();
        //if (errorMessage != null) return errorMessage;

        if (request.AreaId.HasValue)
        {
            if (District.All.All(x => x.Id != request.AreaId.Value))
            {
                return ApiRspCode.BadRequest;
            }
        }

        var user = await userManager.GetUserAsync();
        if (user == null) return ApiRspCode.Unauthorized;

        if (user.BirthDate.HasValue && !request.BirthDate.HasValue)
        {
            return ApiRspCode.BadRequest;
        }

        user.NickName = request.NickName;
        //user.UserInfo.Avatar = request.Avatar;
        user.PersonalizedSignature = request.PersonalizedSignature;
        user.Gender = request.Gender;
        user.BirthDate = request.BirthDate;
        //user.BirthDateTimeZone = request.BirthDateTimeZone;
        user.AreaId = request.AreaId;
        await userManager.RefreshUserInfoCacheAsync(user);
        var r = await userManager.UpdateUserAsync(user);
        if (r.Succeeded)
        {
            return true;
        }
        return r.Fail();
    }
}
