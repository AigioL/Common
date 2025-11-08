using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;

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
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize)
            .WithRequiredSecurityKey();

        #region 刷新用户信息

        routeGroup.MapGet("refreshuserinfo", async (HttpContext context) =>
        {
            var r = await RefreshUserInfoAsync(context);
            return r;
        }).WithDescription("刷新用户信息");
        routeGroup.MapPost("refreshuserinfo", async (HttpContext context) =>
        {
            var r = await RefreshUserInfoAsync(context);
            return r;
        }).ExcludeFromDescription();

        #endregion

        #region 邮箱

        routeGroup.MapPost("sendbindemail", async (HttpContext context) =>
        {
            ApiRsp r = "TODO: 待完成";
            return r;
        }).WithDescription("发送绑定邮箱邮件");
        routeGroup.MapPost("bindemail", async (HttpContext context) =>
        {
            ApiRsp r = "TODO: 待完成";
            return r;
        }).WithDescription("绑定邮箱");
        routeGroup.MapPost("changebindemail", async (HttpContext context) =>
        {
            ApiRsp r = "TODO: 待完成";
            return r;
        }).WithDescription("换绑邮箱");

        #endregion

        #region 换绑手机（安全验证）/绑定新手机号

        routeGroup.MapPost("changebindphonenumber", async (HttpContext context) =>
        {
            var r = await RefreshUserInfoAsync(context);
            return r;
        }).WithDescription("换绑手机（安全验证）V1");
        routeGroup.MapPut("changebindphonenumber", async (HttpContext context) =>
        {
            var r = await RefreshUserInfoAsync(context);
            return r;
        }).WithDescription("绑定新手机号");

        #endregion

        routeGroup.MapDelete("deleteaccount", async (HttpContext context) =>
        {
            var r = await DeleteAccountCoreAsync<TIdentityDbContext>(context);
            return r;
        }).WithDescription("注销（删除）账号");
        routeGroup.MapPost("clockin", async (HttpContext context) =>
        {
            ApiRsp r = "TODO: 待完成";
            return r;
        }).WithDescription("每日签到");
        routeGroup.MapGet("clockinrecords", async (HttpContext context) =>
        {
            ApiRsp r = "TODO: 待完成";
            return r;
        }).WithDescription("获取每日签到记录");
        routeGroup.MapPost("bindphonenumber", async (HttpContext context) =>
        {
            var r = await RefreshUserInfoAsync(context);
            return r;
        }).WithDescription("绑定手机号");
        routeGroup.MapPost("setPassword", async (HttpContext context) =>
        {
            var r = await RefreshUserInfoAsync(context);
            return r;
        }).WithDescription("设置账号密码");
        routeGroup.MapPost("edituserprofile", async (HttpContext context) =>
        {
            var r = await RefreshUserInfoAsync(context);
            return r;
        }).WithDescription("编辑个人资料");
        routeGroup.MapGet("signout", async (HttpContext context) =>
        {
            var r = await SignOutCoreAsync<TIdentityDbContext>(context);
            return r;
        }).WithDescription("退出登录（登出）");
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
        }).WithDescription("解绑账号");
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
        var userInfoDTO = await userManager.GetUserInfoCacheAsync();
        return userInfoDTO;
    }

    //#region 换绑手机（安全验证）/绑定新手机号

    ///// <inheritdoc cref="ChangeBindPhoneNumberCoreAsync(ISmsSender, IJWTUserManagerV2C, IAuthMessageRecordRepository, IIdentityDbContext, ChangePhoneNumberValidationRequest)"/>
    //[HttpPost("changebindphonenumber")]
    //public Task<ApiRspImpl<string?>> ChangeBindPhoneNumber([FromBody] ChangePhoneNumberValidationRequest request)
    //    => ChangeBindPhoneNumberCoreAsync(smsSender, userManager,
    //        authMessageRecordRepo, db, request);

    ///// <inheritdoc cref="ChangeBindPhoneNumberCoreAsync(ISmsSender, IJWTUserManagerV2C, IAuthMessageRecordRepository, IIdentityDbContext, ChangePhoneNumberValidationRequest)"/>
    //[HttpPut("changebindphonenumber")]
    //public Task<ApiRspImpl> ChangeBindPhoneNumber([FromBody] ChangePhoneNumberNewRequest request)
    //     => ChangeBindPhoneNumberCoreAsync(smsSender, userManager,
    //        authMessageRecordRepo, db, request);

    //#endregion

    ///// <inheritdoc cref="DeleteAccountCoreAsync(ControllerBase, IIdentityDbContext, IUserCancelRepository,IDistributedCache,IConnectionMultiplexer)"/>
    //[HttpDelete("deleteaccount")]
    //public Task<ApiRspImpl> DeleteAccount()
    //    => DeleteAccountCoreAsync(this, db, userCancelRepo, cache, connection);

    ///// <inheritdoc cref="ClockInCoreAsync(IJWTUserManagerV2C, IIdentityDbContext, IClockInRecordRepository, ClockInRequest,IConnectionMultiplexer)"/>
    //[HttpPost("clockin")]
    //public Task<ApiRspImpl<ClockInResponse?>> ClockIn([FromBody] ClockInRequest request)
    //    => ClockInCoreAsync(userManager, db, clockInRecordRepo, request, connection);

    ///// <inheritdoc cref="ClockInLogsCoreAsync(IJWTUserManagerV2C, IClockInRecordRepository, DateTimeOffset?)"/>
    //[HttpGet("clockinrecords")]
    //public Task<ApiRspImpl<DateTimeOffset[]?>> ClockInLogs([FromQuery] DateTimeOffset? time)
    //    => ClockInLogsCoreAsync(userManager, clockInRecordRepo, time);

    ///// <inheritdoc cref="BindPhoneNumberCoreAsync(ISmsSender, IJWTUserManagerV2C, IIdentityDbContext, IAuthMessageRecordRepository, BindPhoneNumberRequest)"/>
    //[HttpPost("bindphonenumber")]
    //public Task<ApiRspImpl> BindPhoneNumber([FromBody] BindPhoneNumberRequest request)
    //    => BindPhoneNumberCoreAsync(smsSender, userManager, db,
    //        authMessageRecordRepo, request);

    //[HttpPost("setPassword")]
    //public Task<ApiRspImpl> SetPassword([FromBody] SetPasswordRequest request)
    //    => SetPasswordCoreAsync(smsSender, userManager, db, authMessageRecordRepo, request);

    ///// <inheritdoc cref="EditUserProfileCoreAsync(ControllerBase, IJWTUserManagerV2C, IIdentityDbContext, EditUserProfileRequest)"/>
    //[HttpPost("edituserprofile")]
    //public Task<ApiRspImpl> EditUserProfile([FromBody] EditUserProfileRequest request)
    //    => EditUserProfileCoreAsync(this, userManager, db, request);

    ///// <inheritdoc cref="SignOutCoreAsync(ControllerBase, IIdentityDbContext,IDistributedCache,IConnectionMultiplexer)"/>
    //[HttpGet("signout")]
    //public new Task<ApiRspImpl> SignOut()
    //    => SignOutCoreAsync(this, db, cache, connection);

    ///// <inheritdoc cref="UnbundleAccountCoreAsync(IJWTUserManagerV2C, ExternalLoginChannel)"/>
    //[HttpDelete("unbundleaccount/{channel}")]
    //public Task<ApiRspImpl> UnbundleAccount([FromRoute] ExternalLoginChannel channel)
    //    => UnbundleAccountCoreAsync(userManager, channel);

    //[HttpPost("sendbindemail")]
    //public Task<ApiRspImpl> SendBindEmail([FromBody] BindEmailRequest request) =>
    //    SendBindEmailCoreAsync(smsSender, userManager, authMessageRecordRepo, cache, connection, request.Email, request.SmsCode, request.IsSms);

    //[AllowAnonymous]
    //[HttpPost("bindemail")]
    //public Task<ApiRspImpl> BindEmail([FromBody] EmailConfirmRequest request)
    //    => BindEmailCoreAsync(userManager, cache, request.UserId, request.Token, request.NewEmail);

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

    /// <summary>
    /// 解绑账号
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
        await redisdb.HashDeleteAsync(CacheKeys.IdentityUserJsonWebTokenInfoHashKey, jwtUserIdS);
        await cache.RemoveAsync(jwtUserIdS);
        await db.UserRefreshJsonWebTokens.Where(x => x.Id == jwtUserId).ExecuteDeleteAsync();
    }

    ///// <summary>
    ///// 获取每日签到记录，用于 UI 上显示日历并且标记 ✅
    ///// </summary>
    //internal static async Task<ApiRsp<DateTimeOffset[]?>> ClockInLogsCoreAsync(
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
}
