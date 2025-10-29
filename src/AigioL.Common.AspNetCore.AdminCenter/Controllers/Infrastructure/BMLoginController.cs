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
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;

/// <summary>
/// 管理后台用户登录
/// </summary>
public static partial class BMLoginController
{
    public static void MapBMLogin<TUser>(this IEndpointRouteBuilder b, [StringSyntax("Route")] string pattern = "bm/login") where TUser : BMUser
    {
        b.MapPost(pattern, async (HttpContext context, [FromBody] string[] args) =>
        {
            var r = await LoginAsync<TUser>(context, args);
            return r.SetHttpContext(context);
        })
        .AllowAnonymous()
        .WithDescription("管理后台用户登录");
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
        var o = context.RequestServices.GetRequiredService<IOptions<IdentityOptions>>().Value;

        var ipCacheKey = $"BM_Login_Ip_[{ip}]";
        var ipAccessFailedCountB = await cache.GetAsync(ipCacheKey, context.RequestAborted);
        var ipAccessFailedCount = BinaryPrimitives.ReadInt32BigEndian(ipAccessFailedCountB);
        if (ipAccessFailedCount >= MaxIpAccessFailedCount)
        {
            return HttpStatusCode.TooManyRequests;
        }

        var result = await LoginCoreAsync<TUser>(context, args, cache);
        if (!result.IsSuccess)
        {
            Span<byte> ipAccessFailedCountS = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(ipAccessFailedCountS, ipAccessFailedCount + 1);
            await cache.SetAsync(ipCacheKey, ipAccessFailedCountS.ToArray(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = o.Lockout.DefaultLockoutTimeSpan,
            }, CancellationToken.None);
        }

        return result;
    }

    static async Task<BMApiRsp<JsonWebTokenValue?>> LoginCoreAsync<TUser>(HttpContext context, string[] args, IDistributedCache cache) where TUser : BMUser
    {
        var jwtValueProvider = context.RequestServices.GetRequiredService<IJsonWebTokenValueProvider>();
        var appSettings = context.RequestServices.GetRequiredService<IOptions<BMAppSettings>>().Value;

        var rsaPrivateKey = appSettings.AdminRSAPrivateKey;
        ArgumentNullException.ThrowIfNull(rsaPrivateKey);
        var rsaParameters = RSAUtils.ReadParameters(rsaPrivateKey);
        using var rsa = RSA.Create(rsaParameters);

        var userName = rsa.BMDecrypt(args[0]);
        var password = rsa.BMDecrypt(args[1]);
        var twoFactorCode = args.Length >= 3 ? rsa.BMDecrypt(args[2]) : null;
        var twoFactorRecoveryCode = args.Length >= 4 ? rsa.BMDecrypt(args[3]) : null;

        // https://github.com/dotnet/aspnetcore/blob/v9.0.8/src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs#L90
        var signInManager = context.RequestServices.GetRequiredService<SignInManager<TUser>>();
        signInManager.AuthenticationScheme = BMMinimalApis.BearerScheme;
        const bool isPersistent = false;/*(useCookies == true) && (useSessionCookies != true)*/;

        var user = await signInManager.UserManager.FindByNameAsync(userName);
        if (user == null)
        {
            return ResponseDataUserNameNotFoundOrPasswordInvalid;
        }

        var result = await signInManager.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure: true);
        if (result.RequiresTwoFactor)
        {
            if (!string.IsNullOrEmpty(twoFactorCode))
            {
                result = await signInManager.TwoFactorAuthenticatorSignInAsync(twoFactorCode, isPersistent, rememberClient: isPersistent);
            }
            else if (!string.IsNullOrEmpty(twoFactorRecoveryCode))
            {
                result = await signInManager.TwoFactorRecoveryCodeSignInAsync(twoFactorRecoveryCode);
            }
        }

        if (result.IsLockedOut)
        {
            return "该账号已被锁定";
        }
        if (result == SignInResult.Failed)
        {
            return ResponseDataUserNameNotFoundOrPasswordInvalid;
        }
        if (!result.Succeeded)
        {
            return result.ToString();
            //return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
        }

        // 登录成功，生成 JWT 返回
        var token = await GenerateTokenAsync(context, jwtValueProvider, signInManager.UserManager, user);
        return token;
    }

    static async Task<JsonWebTokenValue> GenerateTokenAsync<TUser>(HttpContext context, IJsonWebTokenValueProvider jwtValueProvider, UserManager<TUser> userManager, TUser user) where TUser : BMUser
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
