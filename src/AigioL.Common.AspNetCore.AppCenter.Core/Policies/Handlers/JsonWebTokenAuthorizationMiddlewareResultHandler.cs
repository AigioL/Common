using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Models;
using MemoryPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using StackExchange.Redis;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using static AigioL.Common.AspNetCore.AppCenter.Policies.Handlers.IJsonWebTokenAuthorizationMiddlewareResultHandler;

namespace AigioL.Common.AspNetCore.AppCenter.Policies.Handlers;

/// <summary>
/// https://learn.microsoft.com/zh-cn/aspnet/core/security/authorization/customizingauthorizationmiddlewareresponse
/// </summary>
public sealed class JsonWebTokenAuthorizationMiddlewareResultHandler<TDbContext> :
    IAuthorizationMiddlewareResultHandler, IJsonWebTokenAuthorizationMiddlewareResultHandler
    where TDbContext : IIdentityDbContext
{
    readonly AuthorizationMiddlewareResultHandler defaultHandler = new();

    static async Task Fail(HttpContext context, ApiRspCode failCode, bool writeApiRsp = false)
    {
        const int statusCode = StatusCodes.Status401Unauthorized;
        context.Response.StatusCode = statusCode;

        context.Items[KEY_FAIL_CODE] = failCode;

        if (writeApiRsp)
        {
            // 旧版使用 MVC ObjectResult，虽然传递了 object? value 值，但实际上并没有写入响应
            var message = failCode switch
            {
                ApiRspCode.UserDeviceIsNotTrust => UserIsBanErrorMessage,
                _ => string.Format(AuthorizationFailErrorMessage_, failCode),
            };
            var traceId = context.GetTraceId();
            ApiRsp apiRsp = new()
            {
                Code = unchecked((uint)failCode),
                Url = context.Request.Path,
                TraceId = traceId,
                Message = message,
            };
            await apiRsp.WriteAsync(
                context.Response,
                cancellationToken: context.RequestAborted);
        }
    }

    async Task<UserDeviceIsTrustWithUserId?> GetUserDeviceIsTrustMapAsync(
        TDbContext dbContext,
        Guid jwtId,
        CancellationToken cancellationToken)
    {
        var query = dbContext.UserJsonWebTokens.AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == jwtId && x.UserDevice != null)
            .Select(x => new UserDeviceIsTrustWithUserId(x.UserDevice.UserId, x.UserDevice.IsTrust));

        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Task EndHandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult,
        bool hasAllowAnonymous,
        ApiRspCode failCode)
    {
        if (hasAllowAnonymous)
        {
            return defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
        else
        {
            return Fail(context, failCode);
        }
    }

    async Task<(UserDeviceIsTrustWithUserId? isTrustMap, bool isTrustMapBinHasValue)> HashGetAsync(IDatabase db, string jwtIdStr)
    {
        // 先尝试从缓存中获取
        var isTrustMapBin = await db.HashGetAsync(CacheKeys.IdentityUserDeviceIsTrustWithUserIdMapHashKey, jwtIdStr);
        if (isTrustMapBin.HasValue)
        {
            var isTrustMapBinLocal = (byte[]?)isTrustMapBin;
            if (isTrustMapBinLocal?.Length > 0)
            {
                var isTrustMap = MemoryPackSerializer.Deserialize<UserDeviceIsTrustWithUserId>(isTrustMapBinLocal);
                if (isTrustMap != null)
                {
                    return (isTrustMap, true);
                }
            }
        }
        return (null, false);
    }

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        var request = context.Request;

        var hasAllowAnonymous = HasAllowAnonymous(context);

        var authHeaderValue = request.Headers.Authorization;
        if (StringValues.IsNullOrEmpty(authHeaderValue))
        {
            await EndHandleAsync(next, context, policy, authorizeResult, hasAllowAnonymous,
                ApiRspCode.MissingAuthorizationHeader);
            return;
        }

        var authHeader = AuthenticationHeaderValue.Parse(authHeaderValue.ToString());
        if (!string.Equals(MSMinimalApis.BearerScheme, authHeader.Scheme, StringComparison.InvariantCultureIgnoreCase))
        {
            await EndHandleAsync(next, context, policy, authorizeResult, hasAllowAnonymous,
                ApiRspCode.AuthSchemeNotCorrect);
        }

        var nameId = context.User.FindFirst(ClaimTypes.NameIdentifier);
        var jwtIdStr = nameId?.Value;
        if (jwtIdStr == null || !ShortGuid.TryParse(jwtIdStr, out Guid jwtId) || jwtId == default)
        {
            await EndHandleAsync(next, context, policy, authorizeResult, hasAllowAnonymous,
                ApiRspCode.UserNotFound);
            return;
        }

        var connection = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        var db = connection.GetDatabase(CacheKeys.RedisHashDataDb);

        // 先尝试从缓存中获取
        (var isTrustMap, bool isTrustMapBinHasValue) = await HashGetAsync(db, jwtIdStr);
        if (isTrustMapBinHasValue)
        {
            // 已从缓存中获取到，isTrustMap 值不可能为 null
        }
        else
        {
            // 缓存中获取失败时，从数据库中取
            var dbContext = context.RequestServices.GetRequiredService<TDbContext>();
            isTrustMap = await GetUserDeviceIsTrustMapAsync(dbContext, jwtId, context.RequestAborted);
            var isTrustMapBinLocal = MemoryPackSerializer.Serialize(isTrustMap);
            await db.HashSetAsync(CacheKeys.IdentityUserDeviceIsTrustWithUserIdMapHashKey, jwtIdStr, isTrustMapBinLocal);
        }

        // 如果数据库中也没有，说明用户不存在
        if (isTrustMap == null)
        {
            await EndHandleAsync(next, context, policy, authorizeResult, hasAllowAnonymous,
                ApiRspCode.UserNotFound);
            return;
        }
        if (!isTrustMap.IsTrust)
        {
            await EndHandleAsync(next, context, policy, authorizeResult, hasAllowAnonymous,
                ApiRspCode.UserDeviceIsNotTrust);
            return;
        }

        request.HttpContext.Items[KEY_USER_ID] = isTrustMap.UserId;
        request.HttpContext.Items[KEY_USER_JWT_ID] = jwtId;

        await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }

    /// <summary>
    /// https://github.com/dotnet/aspnetcore/blob/v5.0.3/src/Mvc/Mvc.Core/src/Authorization/AuthorizeFilter.cs#L221
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool HasAllowAnonymous(HttpContext context)
    {
        //var filters = context.Filters;
        //for (var i = 0; i < filters.Count; i++)
        //    if (filters[i] is IAllowAnonymousFilter)
        //        return true;

        // When doing endpoint routing, MVC does not add AllowAnonymousFilters for AllowAnonymousAttributes that
        // were discovered on controllers and actions. To maintain compat with 2.x,
        // we'll check for the presence of IAllowAnonymous in endpoint metadata.
        var endpoint = context./*HttpContext.*/GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            return true;

        return false;
    }
}

internal interface IJsonWebTokenAuthorizationMiddlewareResultHandler
{
    protected const string KEY_FAIL_CODE = "KEY_FAIL_CODE";
    internal const string KEY_USER_JWT_ID = "KEY_USER_JWT_ID";
    internal const string KEY_USER_ID = "KEY_USER_ID";

    protected const string UserIsBanErrorMessage = "账号已被封禁";
    protected const string AuthorizationFailErrorMessage_ = "服务端错误 {0} - 登录凭证失效，请重新登录";
}