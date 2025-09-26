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
}