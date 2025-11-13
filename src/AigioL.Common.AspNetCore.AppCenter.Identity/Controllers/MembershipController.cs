using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.Models;
using MemoryPack;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using R = AigioL.Common.AspNetCore.AppCenter.Identity.UI.Properties.Resources;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

/// <summary>
/// 用户会员终结点
/// </summary>
public static partial class MembershipController
{
    public static void MapIdentityMembershipV5(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v5/membership")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize);

        routeGroup.MapGet("info", async (HttpContext context) =>
        {
            var r = await GetUserMembershipInfoAsync(context, context.RequestAborted);
            return r;
        }).WithDescription("获取会员信息")
        .WithRequiredSecurityKey();
    }

    /// <summary>
    /// 获取会员信息
    /// </summary>
    /// <returns></returns>
    static async Task<ApiRsp<MembershipInfo?>> GetUserMembershipInfoAsync(
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        var userId = context.GetUserId();
        if (userId == null)
            return ApiRspCode.Unauthorized;

        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(MembershipController));
        var userMembershipRepo = context.RequestServices.GetRequiredService<IUserMembershipRepository>();
        var connection = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();

        var database = connection.GetDatabase(CacheKeys.RedisMessagingDb);

        var cacheKey = CacheKeys.GetUserMembershipCacheKey(userId.Value);
        ReadOnlyMemory<byte> data = await database.StringGetAsync(cacheKey);

        MembershipInfo? r = null;
        if (data.Length <= 0)
        {
            var lockKey = CacheKeys.GetUserMembershipCacheLockKey(userId.Value);
            var lockValue = Guid.NewGuid().ToString();
            var lockDb = connection.GetDatabase(CacheKeys.RedisLockDb);
            var lockTake = await lockDb.LockTakeAsync(cacheKey, lockValue, TimeSpan.FromMinutes(1));
            if (lockTake)
            {
                try
                {
                    // 二次检查
                    data = await database.StringGetAsync(cacheKey);
                    if (data.Length <= 0)
                    {
                        r = await userMembershipRepo.GetUserMembershipAsync(userId.Value, cancellationToken);

                        // 用户不存在会员信息时，返回空对象
                        if (r == null)
                        {
                            return new MembershipInfo();
                        }
                        else
                        {
                            var serializeData = MemoryPackSerializer.Serialize(r);

                            var defaultExpireTime = TimeSpan.FromMinutes(5);
                            if (r.IsMembership)
                            {
                                var expire = r.ExpireDate!.Value - DateTimeOffset.Now;
                                if (expire < defaultExpireTime)
                                    defaultExpireTime = expire;
                            }
                            await database.StringSetAsync(cacheKey, serializeData, defaultExpireTime);
                            return r;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogErrorOnGetUserMembership(logger, ex);
                }
                finally
                {
                    await lockDb.LockReleaseAsync(cacheKey, lockValue);
                }
            }
            else
            {
                return R.操作太频繁请稍后再试;
            }
        }

        if (data.Length > 0)
        {
            r = MemoryPackSerializer.Deserialize<MembershipInfo>(data.Span);
        }

        return r;
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "GetUserMembership fail")]
    private static partial void LogErrorOnGetUserMembership(
        ILogger logger, Exception ex);
}
