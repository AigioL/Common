using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static AigioL.Common.AspNetCore.AppCenter.Policies.Handlers.IJsonWebTokenAuthorizationMiddlewareResultHandler;

namespace Microsoft.AspNetCore.Http;

public static partial class HttpContextExtensions
{
    /// <summary>
    /// 从 HTTP 上下文中获取用户的 JWT ID
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid? GetJwtUserId(this HttpContext context)
    {
        if (context.Items.TryGetValue(KEY_USER_JWT_ID, out var jwtUserIdObj))
        {
            if (jwtUserIdObj is Guid jwtUserId)
            {
                return jwtUserId;
            }
        }
        return null;
    }

    /// <summary>
    /// 从 HTTP 上下文中获取用户的 JWT ID
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid GetJwtUserIdThrowIfNull(this HttpContext context)
    {
        var jwtUserId = context.GetJwtUserId();
        ArgumentNullException.ThrowIfNull(jwtUserId);
        return jwtUserId.Value;
    }

    /// <summary>
    /// 从 HTTP 上下文中获取用户的 ID
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid? GetUserId(this HttpContext context)
    {
        if (context.Items.TryGetValue(KEY_USER_ID, out var userIdObj))
        {
            if (userIdObj is Guid userId)
            {
                return userId;
            }
        }
        return null;
    }

    /// <summary>
    /// 从 HTTP 上下文中获取用户的 ID
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid GetUserIdThrowIfNull(this HttpContext context)
    {
        var userId = context.GetJwtUserId();
        ArgumentNullException.ThrowIfNull(userId);
        return userId.Value;
    }

    const string KEY_APP_VERSION = "KEY_APP_VERSION";

    /// <summary>
    /// 从 HTTP 上下文中获取客户端应用程序的版本信息
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task<IReadOnlyAppVer?> GetAppVerAsync(this HttpContext context)
    {
        if (context.Items.TryGetValue(KEY_APP_VERSION, out var appVerObj))
        {
            if (appVerObj is IReadOnlyAppVer appVer)
            {
                // 从 HTTP 上下文缓存中获取
                return appVer;
            }
        }

        // 从服务接口中获取
        {
            var appVerCoreService = context.RequestServices.GetRequiredService<IAppVerCoreService>();
            var appVer = await appVerCoreService.GetAsync(context, fromHeaderOrQuery: true);
            return appVer;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetTraceId(this HttpContext context)
    {
        // https://github.com/dotnet/aspnetcore/blob/v9.0.8/src/Http/Http.Extensions/src/DefaultProblemDetailsWriter.cs#L58
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        return traceId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetExceptionHandlerPath(this HttpContext context, out Exception? error)
    {
        error = null;
        string path;
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature != null)
        {
            path = exceptionHandlerPathFeature.Path;
            error = exceptionHandlerPathFeature.Error;
        }
        else
        {
            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (exceptionHandlerFeature != null)
            {
                path = exceptionHandlerFeature.Path;
                error = exceptionHandlerFeature.Error;
            }
            else
            {
                path = context.Request.Path;
            }
        }
        return path;
    }
}
