using AigioL.Common.Extensions.Http.Proxy.Models;
using AigioL.Common.Extensions.Http.Proxy.Services.Abstractions;
using AigioL.Common.Models;
using MemoryPack;
using StackExchange.Redis;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using static AigioL.Common.Extensions.Http.Proxy.Services.Abstractions.IWebProxyPoolService;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        CancellationToken cancellationToken = default)
    {
        var db = connection.GetDatabase();
        var proxy = await db.HashGetAsync(KeyWebProxies, proxyId);
        if (proxy.HasValue)
        {
            var m = MemoryPackSerializer.Deserialize<WebProxyModel>(proxy);
            if (m != null)
            {
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

    public async Task<string?> GetProxyIdAsync(
        string userId,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var db = connection.GetDatabase();

        // 有占用的直接返回
        var proxyIdByOccupy = await db.HashGetAsync(KeyUserIdToProxyId, userId);
        if (proxyIdByOccupy.HasValue)
        {
            string? proxyIdByOccupyS = proxyIdByOccupy;
            if (!string.IsNullOrWhiteSpace(proxyIdByOccupyS))
            {
                return proxyIdByOccupyS;
            }
        }

        var array = await db.SortedSetRangeByScoreAsync(KeySortedSetProxyOccupy, take: 1);
        if (array == null || array.Length == 0 || !array[0].HasValue)
        {
            return null;
        }

        string? proxyId = array[0];
        if (string.IsNullOrEmpty(proxyId))
        {
            return null;
        }

        // 增加占用计数
        await db.SortedSetIncrementAsync(KeySortedSetProxyOccupy, proxyId, 1);

        // 设置用户 Id 到代理 Id 的映射
        await db.HashFieldSetAndSetExpiryAsync(
            KeyUserIdToProxyId,
            userId,
            proxyId,
            expiry);

        return proxyId;
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
