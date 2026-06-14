using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.JsonWebTokens.Services.Abstractions;
using AigioL.Common.Primitives.Columns;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;

/// <summary>
/// 管理后台用户登录
/// </summary>
public static partial class BMLoginController
{
    public static void MapBMLogin<TUser>(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "bm/login",
        bool enablePwdLogin = true,
        bool enableSmsLogin = true)
        where TUser : BMUser
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("管理后台用户登录");

        if (enablePwdLogin)
        {
            routeGroup.MapPost("", async (HttpContext context, [FromBody] string[] args) =>
            {
                var r = await LoginAsync<TUser>(context, args);
                return r.SetHttpContext(context);
            })
            .AllowAnonymous()
            .WithDescription("管理后台用户登录（密码）");
        }

        if (enableSmsLogin)
        {
            //routeGroup.MapIdentityVerificationCodesV5_BM<TAppSettings>();

            //routeGroup.MapPost("sms", async (HttpContext context, [FromBody] string[] args) =>
            //{
            //    var r = await LoginBySmsAsync<TUser>(context, args);
            //    return r.SetHttpContext(context);
            //})
            //.AllowAnonymous()
            //.WithDescription("管理后台用户登录（短信）");
        }
    }

    const int MaxIpAccessFailedCount = 10;
    const string ResponseDataUserNameNotFoundOrPasswordInvalid = "用户名不存在或密码错误";

    static async Task<BMApiRsp<JsonWebTokenValue?>> LoginAsync<TUser>(HttpContext context, string[] args) where TUser : BMUser
    {
        if (args.Length < 2)
        {
            return HttpStatusCode.BadRequest;
        }

        var ip = context.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrWhiteSpace(ip))
        {
            return "未知的 IP 地址";
        }

        var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
        var options = context.RequestServices.GetRequiredService<IOptions<IdentityOptions>>().Value;
        var userManager = context.RequestServices.GetRequiredService<UserManager<TUser>>();

        TUser? user = null;

        // IP Redis 键检查周期内失败次数过多限制
        var ipCacheKey = $"BM_Login_Ip_[{ip}]";
        var ipAccessFailedCountB = await cache.GetAsync(ipCacheKey, context.RequestAborted);
        var ipAccessFailedCount = ipAccessFailedCountB == null ? 0 : BinaryPrimitives.ReadInt32BigEndian(ipAccessFailedCountB);
        if (ipAccessFailedCount >= MaxIpAccessFailedCount)
        {
            return HttpStatusCode.TooManyRequests;
        }

        var result = await LoginCoreAsync();
        if (!result.IsSuccess)
        {
            var ipAccessFailedCountS = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(ipAccessFailedCountS, ipAccessFailedCount + 1);

            if (user != null)
            {
                user.AccessFailedCount++;
                await userManager.UpdateAsync(user);
            }

            await cache.SetAsync(ipCacheKey, ipAccessFailedCountS, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = options.Lockout.DefaultLockoutTimeSpan,
            }, CancellationToken.None);
        }

        return result;

        async Task<BMApiRsp<JsonWebTokenValue?>> LoginCoreAsync()
        {
            var jwtValueProvider = context.RequestServices.GetRequiredService<IJsonWebTokenValueProvider>();
            var appSettings = context.RequestServices.GetRequiredService<IOptions<BMAppSettings>>().Value;

            var rsaPrivateKey = appSettings.AdminRSAPrivateKey;
            ArgumentNullException.ThrowIfNull(rsaPrivateKey);
            var rsaParameters = RSAUtils.ReadParameters(rsaPrivateKey);
            using var rsa = RSA.Create(rsaParameters);

            var userName = rsa.BMDecrypt(args[0]);
            var password = rsa.BMDecrypt(args[1]);
            //var twoFactorCode = args.Length >= 3 ? rsa.BMDecrypt(args[2]) : null;
            //var twoFactorRecoveryCode = args.Length >= 4 ? rsa.BMDecrypt(args[3]) : null;

            user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return ResponseDataUserNameNotFoundOrPasswordInvalid;
            }

            var checkPassword = await userManager.CheckPasswordAsync(user, password);
            if (!checkPassword)
            {
                return ResponseDataUserNameNotFoundOrPasswordInvalid;
            }

            var isLockedOut = await userManager.IsLockedOutAsync(user);
            if (user.Disable || isLockedOut)
            {
                return "该账号已被锁定";
            }

            // 登录成功，生成 JWT 返回
            var token = await GenerateTokenAsync(context, jwtValueProvider, userManager, user);
            return token;
        }
    }

    //static async Task<BMApiRsp<JsonWebTokenValue?>> LoginBySmsAsync<TUser>(HttpContext context, string[] args) where TUser : BMUser
    //{
    //    throw new NotImplementedException("TODO");
    //}

    static async Task<JsonWebTokenValue> GenerateTokenAsync<TUser>(
        HttpContext context,
        IJsonWebTokenValueProvider jwtValueProvider,
        UserManager<TUser> userManager,
        TUser user)
        where TUser : BMUser
    {
        var roles = await userManager.GetRolesAsync(user);
        var token = await jwtValueProvider.GenerateTokenAsync(user.Id, roles, aciton: (list) =>
        {
            list.Add(new Claim(nameof(ITenantId.TenantId), user.TenantId.ToString()));
        },
        generateRefreshToken: false,
        cancellationToken: context.RequestAborted);
        return token;
    }
}
