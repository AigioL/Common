using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.Models;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

/// <summary>
/// 登录后用户管理终结点
/// </summary>
public static partial class ManageController
{
    public static void MapIdentityManageV5(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v5/manage")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize)
            .WithRequiredSecurityKey();

        routeGroup.MapPost("refreshuserinfo", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("刷新用户信息");
        routeGroup.MapGet("refreshuserinfo", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("刷新用户信息");
        // changebindemail
        routeGroup.MapPost("changebindphonenumber", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("换绑手机（安全验证）V1");
        routeGroup.MapPut("changebindphonenumber", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("绑定新手机号");
        routeGroup.MapDelete("deleteaccount", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("注销（删除）账号");
        routeGroup.MapPost("clockin", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("每日签到");
        routeGroup.MapGet("clockinrecords", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("获取每日签到记录");
        routeGroup.MapPost("bindphonenumber", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("绑定手机号");
        routeGroup.MapPost("setPassword", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("设置账号密码");
        routeGroup.MapPost("edituserprofile", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("编辑个人资料");
        routeGroup.MapGet("signout", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("退出登录（登出）V1");
        routeGroup.MapDelete("unbundleaccount/{channel}", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("解绑账号");
        routeGroup.MapPost("sendbindemail", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("发送绑定邮箱邮件");
        routeGroup.MapPost("bindemail", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("绑定邮箱");
    }

    static async Task<ApiRsp<UserInfoModel?>> RefreshUserInfo(
        HttpContext context)
    {
        var user = context.GetUserId();
        if (user == null)
            return ApiRspCode.Unauthorized;

        var userManager = context.RequestServices.GetRequiredService<IUserManager2>();
        var userInfoDTO = await userManager.GetUserInfoCacheV1Async();
        return userInfoDTO;
    }

    //[HttpPost("changebindemail")]
    //public async Task<ApiRspImpl> BindEmail([FromBody] EmailConfirmRequest request)
    //{
    //    if (string.IsNullOrEmpty(request.UserId))
    //        return ApiRspCode.Unauthorized;
    //    if (string.IsNullOrEmpty(request.NewEmail) || string.IsNullOrEmpty(request.Token))
    //        return ApiRspCode.RequestModelValidateFail;
    //    if (!request.UserId.TryParseGuid(out var userId))
    //        return ApiRspCode.Unauthorized;
    //    var user = await userManager.FindByIdAsync(userId);
    //    if (user == null)
    //        return "绑定邮件操作出现错误";
    //    if (await userManager.VerifyEmailBoundAsync(user.Email!))
    //        return "邮箱已被其它用户绑定";

    //    var result = await userManager.Thiz.ChangeEmailAsync(user, request.NewEmail, request.Token);
    //    if (result.Succeeded)
    //    {
    //        await userManager.RefreshUserInfoCacheAsync(user);
    //        return ApiRspHelper.Ok();
    //    }
    //    return Fail(result);
    //}

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
}
