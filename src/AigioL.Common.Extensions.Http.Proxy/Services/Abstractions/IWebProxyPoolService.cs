using AigioL.Common.Extensions.Http.Proxy.Models;
using AigioL.Common.Models;
using System.Net;

namespace AigioL.Common.Extensions.Http.Proxy.Services.Abstractions;

/// <summary>
/// Web 代理池服务接口
/// </summary>
public partial interface IWebProxyPoolService
{
    /// <summary>
    /// 使用传入的代理进行连接测试，返回是否成功与连接耗时
    /// </summary>
    Task<ApiRsp<TimeSpan?>> ConnectionTestAsync(
        IWebProxy webProxy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 从缓存中获取所有代理
    /// </summary>
    Task<ReadOnlyMemory<WebProxyModel>> GetWebProxiesByCacheAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 从缓存中根据代理 Id 获取单个代理
    /// </summary>
    Task<WebProxyModel?> GetWebProxyByCacheAsync(
        string proxyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 从数据库中更新代理到缓存
    /// </summary>
    Task UpdateWebProxiesToCacheAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 清理所有缓存值
    /// </summary>
    Task ClearAllCacheAsync(
        CancellationToken cancellationToken = default);
}

partial interface IWebProxyPoolService
{
    /// <summary>
    /// Web 代理的缓存哈希表
    /// </summary>
    protected const string KeyWebProxies = "WebProxyPool_WebProxies";

    /// <summary>
    /// 用户 Id 到代理 Id 的映射缓存哈希表，包含过期时间
    /// </summary>
    protected const string KeyUserIdToProxyId = "WebProxyPool_UserIdToProxyId";

    /// <summary>
    /// 代理 Id 被占用的次数有序集合
    /// </summary>
    protected const string KeySortedSetProxyOccupy = "WebProxyPool_ProxyOccupy_SortedSet";

    /// <summary>
    /// 代理 Id 使用失败的次数有序集合
    /// </summary>
    protected const string KeySortedSetProxyFailCount = "WebProxyPool_ProxyFailCount_SortedSet";
}