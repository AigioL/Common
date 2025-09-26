using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Response;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Columns;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

public static partial class AccountController
{
    public static void MapIdentityAccount(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "api/Account")
    {
        // ApiService.Identity.Controllers;
        // [Route("api/[controller]/[action]")]
        // class AccountController
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous()
            .WithRequiredSecurityKey();

        // [HttpPost]-
        // Task<ApiRspImpl<LoginOrRegisterResponseCompat?>> LoginOrRegister(
        // [FromBody] LoginOrRegisterRequestCompat request)
        routeGroup.MapPost("LoginOrRegister", [Obsolete] async (HttpContext context,
            [FromBody] LoginOrRegisterRequest request) =>
        {
            var r = await LoginOrRegister(context,
                request.PhoneNumber, null,
                request.SmsCode, request.Channel, null);
            return r;
        }).WithDescription("【Obsolete】登录或注册账号 V(-1)");
        routeGroup.MapGet("RefreshToken/{refresh_token}", [Obsolete] async (HttpContext context,
            [FromRoute] string refresh_token) =>
        {
            var r = await RefreshToken(context,
                refresh_token);
            return r;
        }).WithDescription("【Obsolete】刷新 JWT V(-1)");
    }

    public static void MapIdentityAccountV0(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/account")
    {
        // ApiService.Identity.Controllers;
        // [Route("identity/account")]
        // class IdentityAccountController
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous()
            .WithRequiredSecurityKey();

        // [HttpPost("loginorregister")]
        // Task<ApiRspImpl<RcLoginOrRegisterResponseCompat?>> LoginOrRegister(
        // [FromBody] LoginOrRegisterRequest request)
        routeGroup.MapPost("loginorregister", [Obsolete] async (HttpContext context,
            [FromBody] LoginOrRegisterRequestV1 request) =>
        {
            var deviceId = request.GetDeviceId();
            var r = await LoginOrRegister(context,
                request.PhoneNumber, null,
                request.SmsCode, request.Channel, deviceId);
            return r;
        }).WithDescription("【Obsolete】登录或注册账号 V0");

        // validateRegisterEmail
        // resetPassword
        // registerByEmail
        // loginByPassword
        // refreshtoken
    }

    public static void MapIdentityAccountV1(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v1/account")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous()
            .WithRequiredSecurityKey();

        routeGroup.MapPost("loginorregister", [Obsolete] async (HttpContext context,
            [FromBody] LoginOrRegisterRequestV1 request) =>
        {
            var deviceId = request.GetDeviceId();
            var r = await LoginOrRegister(context,
                request.PhoneNumber, null,
                request.SmsCode, request.Channel, deviceId);
            return r;
        }).WithDescription("【Obsolete】登录或注册账号 V1");
    }

    public static void MapIdentityAccountV2(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v2/account")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous()
            .WithRequiredSecurityKey();

        routeGroup.MapPost("loginorregister", async (HttpContext context,
            [FromBody] LoginOrRegisterRequestV2 request) =>
        {
            var deviceId = request.GetDeviceId();
            var r = await LoginOrRegister(context,
                request.PhoneNumber, request.PhoneNumberRegionCode,
                request.SmsCode, request.Channel, deviceId);
            return r;
        }).WithDescription("登录或注册账号 V2");
#if DEBUG
        routeGroup.MapGet("test/ex", async (HttpContext context) =>
        {
            await Task.Delay(10, context.RequestAborted);
            throw new ApplicationException("测试 WithRequiredSecurityKey 的异常页面");
        }).WithIdentityUIView();
#endif

        // serversideproxytoken
        // refreshtoken
    }


    internal const int MaxIpAccessFailedCount = 12;

    /// <summary>
    /// 登录或注册账号，如要支持密码登录，需使用 <see cref="SignInManager{TUser}"/> 提供纪录失败次数与锁定用户
    /// </summary>
    static async Task<ApiRsp<LoginOrRegisterResponse?>> LoginOrRegister(
        HttpContext context,
        string? phoneNumber,
        string? phoneNumberRegionCode,
        string? smsCode,
        LoginChannel loginChannel,
        string? deviceId)
    {
        if (string.IsNullOrWhiteSpace(phoneNumberRegionCode))
        {
            phoneNumberRegionCode = IPhoneNumber.DefaultPhoneNumberRegionCode;
        }

        await Task.CompletedTask;
        throw new NotImplementedException();
        //var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        //if (string.IsNullOrWhiteSpace(ip))
        //    return "未知的 IP 地址";

        //var ipCacheKey = GetIpCacheKey(ip);
        //var ipAccessFailedCount = await cache.GetV2Async<int>(ipCacheKey, HttpContext.RequestAborted);
        //if (ipAccessFailedCount >= MaxIpAccessFailedCount)
        //    return ApiRspCode.TooManyRequests;

        //var result = await LoginOrRegisterCore(request);
        //if (result.IsSuccess)
        //    cache.Remove(ipCacheKey);
        //else
        //{
        //    var lockoutEnd = GetLockoutEnd();
        //    await cache.SetV2Async(ipCacheKey, ipAccessFailedCount + 1,
        //    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = lockoutEnd });
        //}
        //return result;
    }

    /// <summary>
    /// 刷新 JWT
    /// </summary>
    static async Task<ApiRsp<JsonWebTokenValue?>> RefreshToken(
        HttpContext context,
        string refresh_token,
        string? deviceId = null)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }
}

file static class B9490399
{

}