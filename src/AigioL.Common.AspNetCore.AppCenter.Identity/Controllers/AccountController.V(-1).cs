using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Response;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using static AigioL.Common.AspNetCore.AppCenter.Identity.Controllers.AccountController;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

static partial class AccountController
{
    public static void MapIdentityAccount(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "api/Account")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous()
            .WithRequiredSecurityKey();

        routeGroup.MapPost("LoginOrRegister", [Obsolete] async (HttpContext context,
            [FromBody] LoginOrRegisterRequest_V_1 request) =>
        {
            var r = await V_1.LoginOrRegister(context, request);
            return r;
        }).WithDescription("【废弃】登录或注册账号 V(-1)");
        routeGroup.MapGet("RefreshToken/{refresh_token}", [Obsolete] async (HttpContext context,
            [FromRoute] string refresh_token) =>
        {
            const string? deviceId = null;
            var r = await RefreshTokenAsync(context, refresh_token, deviceId);
            return r;
        }).WithDescription("【废弃】刷新 JWT V(-1)");
    }
}

file static class V_1
{
    internal static async Task<ApiRsp<LoginOrRegisterResponse_V_1?>> LoginOrRegister(
         HttpContext context,
         LoginOrRegisterRequest_V_1 request)
    {
        if (!context.GetRemoteIpAddress(out var ip))
        {
            return "未知的 IP 地址";
        }

        var ipCacheKey = GetIpCacheKey(ip);
        var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
        var ipAccessFailedCount = await cache.GetV2Async<int>(ipCacheKey, context.RequestAborted);
        if (ipAccessFailedCount >= MaxIpAccessFailedCount)
        {
            return HttpStatusCode.TooManyRequests;
        }

        var result = await LoginOrRegisterCore(request);
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

    static async Task<ApiRsp<LoginOrRegisterResponse_V_1?>> LoginOrRegisterCore(
        LoginOrRegisterRequest_V_1 request)
    {

    }
}