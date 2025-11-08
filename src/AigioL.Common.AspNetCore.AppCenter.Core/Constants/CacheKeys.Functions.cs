using StackExchange.Redis;

namespace AigioL.Common.AspNetCore.AppCenter.Constants;

static partial class CacheKeys
{
    /// <summary>
    /// 添加文章浏览量
    /// </summary>
    /// <returns></returns>
    public static async Task ArticleViewIncrementAsync(
        Guid id,
        IConnectionMultiplexer connection,
        CancellationToken cancellationToken = default)
    {
        var idString = id.ToString();
        var dbConnection = connection.GetDatabase(RedisHashIncrementDb, cancellationToken);
        await dbConnection.HashIncrementAsync(ArticleViewHashKey, idString);
    }

    /// <summary>
    /// 获取用户会员信息缓存 Key
    /// </summary>
    public static string GetUserMembershipCacheKey(Guid userId) => $"UserMembership:{userId}";

    /// <summary>
    /// 获取用户会员信息缓存锁 Key
    /// </summary>
    public static string GetUserMembershipCacheLockKey(Guid userId) => $"UserMembershipLock:{userId}";
}