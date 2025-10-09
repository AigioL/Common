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
using System.Diagnostics;
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

    static async Task Fail(HttpContext context, ApiRspCode failCode)
    {
        const int statusCode = StatusCodes.Status401Unauthorized;

        context.Items[KEY_FAIL_CODE] = failCode;
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
        context.Response.StatusCode = statusCode;
        await MSMinimalApis.WriteApiRspAsync(
            context.Response,
            apiRsp,
            cancellationToken: context.RequestAborted);
    }

    async Task<UserDeviceIsTrustMap?> GetUserDeviceIsTrustMapAsync(
        TDbContext dbContext,
        Guid jwtId,
        CancellationToken cancellationToken)
    {
        var query = dbContext.UserJsonWebTokens.AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == jwtId && x.UserDevice != null)
            .Select(x => new UserDeviceIsTrustMap(x.UserDevice.UserId, x.UserDevice.IsTrust));

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
        if (!ShortGuid.TryParse(jwtIdStr, out Guid jwtId) || jwtId == default)
        {
            await EndHandleAsync(next, context, policy, authorizeResult, hasAllowAnonymous,
                ApiRspCode.UserNotFound);
            return;
        }

        var connection = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        var db = connection.GetDatabase(CacheKeys.RedisHashDataDb);

        UserDeviceIsTrustMap? isTrustMap = null;
        var isTrustMapBin = await db.HashGetAsync(CacheKeys.IdentityUserIsBanMapHashKey, jwtIdStr);
        if (isTrustMapBin.HasValue)
        {
            try
            {
                var isTrustMapBinLocal = (byte[]?)isTrustMapBin;
                if (isTrustMapBinLocal != null && isTrustMapBinLocal.Length != 0)
                {
                    isTrustMap = MemoryPackSerializer.Deserialize<UserDeviceIsTrustMap>(isTrustMapBinLocal);
                }
            }
            catch
            {
            }
        }
        if (!isTrustMap.HasValue)
        {
            var dbContext = context.RequestServices.GetRequiredService<TDbContext>();
            isTrustMap = await GetUserDeviceIsTrustMapAsync(dbContext, jwtId, context.RequestAborted);
            var isTrustMapBinLocal = MemoryPackSerializer.Serialize(isTrustMap.Value);
            await db.HashSetAsync(CacheKeys.IdentityUserIsBanMapHashKey, jwtIdStr, isTrustMapBinLocal);
        }
        if (!isTrustMap.HasValue)
        {
            await EndHandleAsync(next, context, policy, authorizeResult, hasAllowAnonymous,
                ApiRspCode.UserNotFound);
            return;
        }
        if (!isTrustMap.Value.UserDeviceIsTrust)
        {
            await EndHandleAsync(next, context, policy, authorizeResult, hasAllowAnonymous,
                ApiRspCode.UserDeviceIsNotTrust);
            return;
        }

        request.HttpContext.Items[KEY_USER_ID] = isTrustMap.Value.UserId;
        request.HttpContext.Items[KEY_USER_JWT_ID] = jwtId;
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