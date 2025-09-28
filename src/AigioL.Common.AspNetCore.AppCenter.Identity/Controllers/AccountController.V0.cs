using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Response;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
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
        routeGroup.MapPost("refreshtoken", async (HttpContext context,
            [FromBody] RefreshTokenRequest_V0 request) =>
        {
            var deviceId = request.GetDeviceId();
            var r = await RefreshTokenAsync(context, request.RefreshToken, deviceId);
            return r;
        }).WithDescription("刷新 JWT V0");
    }
}