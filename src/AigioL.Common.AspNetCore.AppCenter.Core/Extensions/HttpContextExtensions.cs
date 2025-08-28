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
}
