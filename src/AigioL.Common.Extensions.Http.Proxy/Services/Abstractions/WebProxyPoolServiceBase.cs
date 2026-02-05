using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.Extensions.Http.Proxy.Models;
using AigioL.Common.Extensions.Http.Proxy.Services.Abstractions;
using AigioL.Common.Models;
using MemoryPack;
using StackExchange.Redis;
using System.Diagnostics;
using System.Net;
using static AigioL.Common.Extensions.Http.Proxy.Services.Abstractions.IWebProxyPoolService;

namespace AigioL.Common.Extensions.Http.Proxy.Services;

/// <summary>
/// 由 Redis 实现的高性能 Web 代理池服务
/// </summary>
/// <param name="connection"></param>
public abstract partial class WebProxyPoolServiceBase(
    IConnectionMultiplexer connection) :
    IWebProxyPoolService
{
    protected string ConnectionTestUrl => "http://www.msftconnecttest.com/connecttest.txt";

    protected TimeSpan expiryTimeSpan = TimeSpan.FromHours(6);

    public async Task<ApiRsp<TimeSpan?>> ConnectionTestAsync(
        IWebProxy webProxy,
        CancellationToken cancellationToken = default)
    {
        using var h = new SocketsHttpHandler
        {
            UseProxy = true,
            Proxy = webProxy,
            AllowAutoRedirect = false,
        };
        using var c = new HttpClient(h);
        using var req = new HttpRequestMessage(HttpMethod.Get, ConnectionTestUrl);

        var sw = Stopwatch.StartNew();

        using var rsp = await c.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        sw.Stop();

        return sw.Elapsed;
    }

    protected abstract Task<WebProxyModel[]> GetWebProxiesAsync(
        CancellationToken cancellationToken = default);

    public async Task<ReadOnlyMemory<WebProxyModel>> GetWebProxiesByCacheAsync(
        CancellationToken cancellationToken = default)
    {
        var db = connection.GetDatabase();
        var all = await db.HashGetAllAsync(KeyWebProxies);

        if (all == null || all.Length == 0)
        {
            return default;
        }
        else
        {
            var proxies = new WebProxyModel[all.Length];
            int index = 0;
            for (int i = 0; i < all.Length; i++)
            {
                var it = all[i];
                if (it.Value.HasValue)
                {
                    var proxy = MemoryPackSerializer.Deserialize<WebProxyModel>(it.Value);
                    if (proxy != null)
                    {
                        proxies[index++] = proxy;
                    }
                }
            }
            return proxies.AsMemory(0, index);
        }
    }

    public async Task<WebProxyModel?> GetWebProxyByCacheAsync(
        string proxyId,
        string? userId,
        CancellationToken cancellationToken = default)
    {
        var db = connection.GetDatabase();
        var proxy = await db.HashGetAsync(KeyWebProxies, proxyId);
        if (proxy.HasValue)
        {
            var m = MemoryPackSerializer.Deserialize<WebProxyModel>(proxy);
            if (m != null)
            {
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    //延长租约时间
                    double expireScore = DateTimeOffset.UtcNow.Add(expiryTimeSpan).ToUnixTimeSeconds();
                    await db.SortedSetUpdateAsync(KeyUserIdToProxyIdTimeout, userId, expireScore);
                }
                return m;
            }
        }
        return null;
    }

    public async Task<WebProxyModel[]> UpdateWebProxiesToCacheAsync(
        CancellationToken cancellationToken = default)
    {
        var db = connection.GetDatabase();
        var proxies = await GetWebProxiesAsync(cancellationToken);
        if (proxies == null || proxies.Length == 0)
        {
        }
        else
        {
            var entries = new HashEntry[proxies.Length];
            for (int i = 0; i < proxies.Length; i++)
            {
                var proxy = proxies[i];
                var data = MemoryPackSerializer.Serialize(proxy);
                entries[i] = new HashEntry(proxy.Id, data);
            }
            await db.HashSetAsync(KeyWebProxies, entries);
            var proxieIds = proxies.Select(x =>
            {
                RedisValue rv = x.Id;
                return new SortedSetEntry(rv, 0);
            }).ToArray();
            await db.SortedSetAddAsync(KeySortedSetProxyOccupy, proxieIds);
            await db.SortedSetAddAsync(KeySortedSetProxyFailCount, proxieIds);
        }
        return proxies ?? [];
    }

    public async Task ClearAllCacheAsync(
        CancellationToken cancellationToken = default)
    {
        var db = connection.GetDatabase();
        await db.KeyDeleteAsync(KeyWebProxies);
        await db.KeyDeleteAsync(KeyUserIdToProxyId);
        await db.KeyDeleteAsync(KeySortedSetProxyOccupy);
        await db.KeyDeleteAsync(KeySortedSetProxyFailCount);
    }

    /// <summary>
    /// 单个代理的最大使用次数阈值
    /// </summary>
    protected virtual int MaxUsage => 50;

    public async Task<string?> GetProxyIdAsync(
        string userId,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var db = connection.GetDatabase();

        // 1. 检查是否存在现有租约 (复用已分配的代理)
        var proxyIdByOccupy = await db.HashGetAsync(KeyUserIdToProxyId, userId);
        if (proxyIdByOccupy.HasValue)
        {
            string? proxyIdByOccupyS = proxyIdByOccupy;
            if (!string.IsNullOrWhiteSpace(proxyIdByOccupyS))
            {
                return proxyIdByOccupyS;
            }
        }

        // 2. [核心优化] 使用 Lua 脚本原子获取并锁定代理
        // 解决了两个问题：
        // a. 并发下多个请求同时读到同一个 proxyId 导致的分配不均。
        // b. 严格的阈值检查 (原子性保证不会突破 maxUsage)。
        var script = @"
            -- KEYS[1]: 代理占用计数 ZSet (KeySortedSetProxyOccupy)
            -- ARGV[1]: 最大占用阈值 (maxUsage)

            -- 1. 获取当前占用最少的一个代理
            -- ZRANGE 返回 {member, score}
            local candidates = redis.call('ZRANGE', KEYS[1], 0, 0, 'WITHSCORES')
            if #candidates == 0 then
                return nil
            end

            local id = candidates[1]
            local score = tonumber(candidates[2])

            -- 2. 阈值检查：如果最低分已经达到或超过上限，说明所有代理都已满
            if score >= tonumber(ARGV[1]) then
                return nil
            end

            -- 3. 原子增加占用计数
            redis.call('ZINCRBY', KEYS[1], 1, id)
            return id
        ";

        var result = await db.ScriptEvaluateAsync(LuaScript.Prepare(script), new
        {
            key1 = (RedisKey)KeySortedSetProxyOccupy,
            arg1 = MaxUsage,
        });

        if (result.IsNull)
        {
            return null; // 所有代理都忙，或池子为空
        }

        string proxyId = result.ToString();

        // 方案一：
        {
            // 设置用户 Id 到代理 Id 的映射
            //await db.HashFieldSetAndSetExpiryAsync(
            //    KeyUserIdToProxyId,
            //    userId,
            //    proxyId,
            //    expiry);
        }

        // 方案二：
        // 不要依赖 Ex (Expire) 机制，而是将“过期时间”作为 Score 存入一个 Sorted Set。 C# 端不再 hold 连接去 Subscribe，而是简单地启动一个定时器（如每秒），调用一次 Lua 脚本进行“清理”。
        // 这种方式是完全原子的，逻辑完全在 Lua 中，且可靠性远高于 Keyspace Notifications。
        {
            // 设置用户 Id 到代理 Id 的映射
            await db.HashSetAsync(KeyUserIdToProxyId, userId, proxyId);

            // 不使用 TTL，而是添加到超时 ZSet 中
            // Score = 当前时间 + 过期时长 (使用 Unix 时间戳)
            double expireScore = DateTimeOffset.UtcNow.Add(expiry).ToUnixTimeSeconds();
            await db.SortedSetAddAsync(KeyUserIdToProxyIdTimeout, userId, expireScore);
        }

        return proxyId;
    }

    public async Task<WebProxyModel?> GetWebProxyAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var db = connection.GetDatabase();
        var proxyId = await GetProxyIdAsync(userId, TimeSpan.FromHours(6), cancellationToken);
        if (string.IsNullOrWhiteSpace(proxyId))
            return null;
        var webProxy = await GetWebProxyByCacheAsync(proxyId, userId, cancellationToken);
        return webProxy;
    }

    public async Task CleanupExpiredProxiesAsync()
    {
        // 使用 ZSet 存储过期时间，通过 Lua 脚本批量处理。这符合原子性要求，解决了“监听事件丢失”的问题，并且将逻辑核心封装在 Lua 中，C# 仅充当一个简单的定时触发器（Ticker）
        var db = connection.GetDatabase();

        // 创建一个 Lua 脚本，用于查找已过期的数据、执行清理并扣减计数。
        // -- KEYS[1]: 记录过期时间的 ZSet (例如 "proxy:timeouts")
        // -- KEYS[2]: 用户与代理映射的 Hash (例如 "user:proxy:map")
        // -- KEYS[3]: 代理占用计数的 ZSet (例如 "proxy:occupy")
        // -- ARGV[1]: 当前 Unix 时间戳 (秒或毫秒)
        // -- 1. 获取所有分数小于等于当前时间的过期项 (UserId)
        // -- 2. 获取该用户当前占用的 ProxyId
        // -- 3. 从 Hash 中移除用户
        // -- 4. 原子递减该 Proxy 的占用计数
        // -- 5. 从超时队列中移除该用户
        var script = @"
            local expired = redis.call('ZRANGEBYSCORE', KEYS[1], '-inf', ARGV[1])
            if #expired > 0 then
                for _, u in ipairs(expired) do
                    local pid = redis.call('HGET', KEYS[2], u)
                    if pid then
                        redis.call('HDEL', KEYS[2], u)
                        redis.call('ZINCRBY', KEYS[3], -1, pid)
                    end
                    redis.call('ZREM', KEYS[1], u)
                end
            end
            return #expired";

        // 获取当前时间戳
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // 执行脚本
        await db.ScriptEvaluateAsync(LuaScript.Prepare(script), new
        {
            key1 = (RedisKey)KeyUserIdToProxyIdTimeout,       // 超时 ZSet
            key2 = (RedisKey)KeyUserIdToProxyId,              // 映射 Hash
            key3 = (RedisKey)KeySortedSetProxyOccupy,         // 计数 ZSet
            arg1 = now                                        // 当前时间阈值
        });
    }

    public async Task<int> MarkFailCountAsync(
        string proxyId,
        CancellationToken cancellationToken = default)
    {
        var db = connection.GetDatabase();
        var newCount = await db.SortedSetIncrementAsync(KeySortedSetProxyFailCount, proxyId, 1);
        return (int)newCount;
    }

    public async Task<Dictionary<string, int>> GetSortedSetProxyOccupyAsync(
        CancellationToken cancellationToken = default)
    {
        var db = connection.GetDatabase();

        var array = await db.SortedSetRangeByScoreWithScoresAsync(KeySortedSetProxyOccupy);
        if (array == null || array.Length == 0)
        {
            return [];
        }
        else
        {
            var results = new Dictionary<string, int>();
            for (int i = 0; i < array.Length; i++)
            {
                var it = array[i];
                if (it.Element.HasValue)
                {
                    string? proxyIdS = it.Element;
                    if (!string.IsNullOrEmpty(proxyIdS))
                    {
                        results.Add(proxyIdS, (int)it.Score);
                    }
                }
            }
            return results;
        }
    }

    public async Task<Dictionary<string, int>> GetSortedSetProxyFailCountAsync(
        CancellationToken cancellationToken = default)
    {
        var db = connection.GetDatabase();

        var array = await db.SortedSetRangeByScoreWithScoresAsync(KeySortedSetProxyFailCount);
        if (array == null || array.Length == 0)
        {
            return [];
        }
        else
        {
            var results = new Dictionary<string, int>();
            for (int i = 0; i < array.Length; i++)
            {
                var it = array[i];
                if (it.Element.HasValue)
                {
                    string? proxyIdS = it.Element;
                    if (!string.IsNullOrEmpty(proxyIdS))
                    {
                        results.Add(proxyIdS, (int)it.Score);
                    }
                }
            }
            return results;
        }
    }
}
