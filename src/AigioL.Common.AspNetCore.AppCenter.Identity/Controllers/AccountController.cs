using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Response;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

public static partial class AccountController
{
    public static void MapIdentityAccount(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v1/account")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous()
            .WithRequiredSecurityKey();

        routeGroup.MapPost("loginorregister", async (HttpContext context,
            [FromBody] LoginOrRegisterRequest request) =>
        {
            var r = await LoginOrRegister(context, request);
            return r;
        }).WithDescription("登录或注册账号");

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
    static async Task<ApiRsp<LoginOrRegisterResponse?>> LoginOrRegister(
        HttpContext context,
        LoginOrRegisterRequest request)
    {
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
}

file static class B9490399
{

}