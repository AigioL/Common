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
        string? userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 从数据库中更新代理到缓存
    /// </summary>
    Task<WebProxyModel[]> UpdateWebProxiesToCacheAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 清理所有缓存值
    /// </summary>
    Task ClearAllCacheAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据用户 Id 获取分配的代理 Id
    /// </summary>
    Task<string?> GetProxyIdAsync(
        string userId,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 触发 Lua 脚本执行原子清理和计数递减
    /// </summary>
    Task CleanupExpiredProxiesAsync();

    /// <summary>
    /// 标记代理使用失败次数加 1，原子操作，返回当前失败次数
    /// </summary>
    Task<int> MarkFailCountAsync(
        string proxyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取代理被占用次数有序集合
    /// </summary>
    Task<Dictionary<string, int>> GetSortedSetProxyOccupyAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取代理使用失败的次数有序集合
    /// </summary>
    Task<Dictionary<string, int>> GetSortedSetProxyFailCountAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 用户分配的 Web 代理
    /// </summary>
    /// <returns></returns>
    Task<WebProxyModel?> GetWebProxyAsync(
        string userId,
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

    protected const string KeyUserIdToProxyIdTimeout = "WebProxyPool_UserIdToProxyIdTimeout";
}