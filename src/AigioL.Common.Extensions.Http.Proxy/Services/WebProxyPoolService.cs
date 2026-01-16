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
public abstract partial class WebProxyPoolService(IConnectionMultiplexer connection) : IWebProxyPoolService
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
            var proxies = await UpdateWebProxiesToCacheCoreAsync(db, cancellationToken);
            return proxies;
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
                index++;
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

    public Task UpdateWebProxiesToCacheAsync(
        CancellationToken cancellationToken = default)
    {
        var db = connection.GetDatabase();
        return UpdateWebProxiesToCacheCoreAsync(db, cancellationToken);
    }

    async Task<WebProxyModel[]> UpdateWebProxiesToCacheCoreAsync(
        IDatabase db,
        CancellationToken cancellationToken = default)
    {
        var proxies = await GetWebProxiesAsync(cancellationToken);
        if (proxies == null || proxies.Length == 0)
        {
            await db.HashSetAsync(KeyWebProxies, [new HashEntry(RedisValue.Null, RedisValue.Null)]);
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
}
