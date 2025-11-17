using StackExchange.Redis;
using System.Buffers;

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

    /// <summary>
    /// 获取方法缓存 Key
    /// </summary>
    public static string GetMethodCacheKey(string methodName, params object?[] parameters)
    {
        const string separator = ", ";
        var parameters2 = parameters.Select(static p => p?.ToString()).ToArray();
        var len = methodName.Length + parameters2.Sum(static x => x == null ? 0 : x.Length) + ((parameters2.Length - 1) * separator.Length) + 2;
        var r = string.Create(len, parameters2, (chars, parameters2) =>
        {
            var temp = chars;
            methodName.AsSpan().CopyTo(temp);
            temp = temp[methodName.Length..];
            temp[0] = '(';
            temp = temp[1..];
            for (int i = 0; i < parameters2.Length; i++)
            {
                var param = parameters2[i];
                if (param != null)
                {
                    param.AsSpan().CopyTo(temp);
                    temp = temp[param.Length..];
                }
                if (i < parameters2.Length - 1)
                {
                    separator.AsSpan().CopyTo(temp);
                    temp = temp[separator.Length..];
                }
            }
            temp[0] = ')';
        });
        return r;
    }
}