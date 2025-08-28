using MemoryPack;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.Caching.Distributed;

/// <summary>
/// 提供由分布式缓存实现的内存缓存接口 <see cref="global::Microsoft.Extensions.Caching.Memory.IMemoryCache"/> 兼容的扩展函数集
/// </summary>
public static partial class CacheExtensions
{
    /// <inheritdoc cref="global::Microsoft.Extensions.Caching.Memory.CacheExtensions.GetOrCreateAsync{TItem}(global::Microsoft.Extensions.Caching.Memory.IMemoryCache, object, Func{global::Microsoft.Extensions.Caching.Memory.ICacheEntry, Task{TItem}})"/>
    public static async Task<TItem?> GetOrCreateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TItem>(
        this IDistributedCache cache,
        string key,
        Func<ICacheEntryCompat, Task<TItem>> factory,
        CancellationToken cancellationToken = default)
    {
        TItem? value;
        var bytes = await cache.GetAsync(key, cancellationToken);
        if (bytes != null)
        {
            if (bytes.Length == 0)
            {
                return default;
            }
            else
            {
                try
                {
                    value = MemoryPackSerializer.Deserialize<TItem>(bytes);
                    return value;
                }
                catch
                {
                    // 反序列化失败，视作缓存数据无效
                }
            }
        }

        var entry = new CacheEntryCompatImpl
        {
            Key = key,
        };
        // 调用传入委托获取数据
        value = await factory(entry);
        // 序列化并存入缓存，默认值视作空
        var isDefaultValue = EqualityComparer<TItem>.Default.Equals(value, default);
        bytes = isDefaultValue ? [] : MemoryPackSerializer.Serialize(value);
        await cache.SetAsync(key, bytes, entry, cancellationToken);
        return value;
    }

    /// <inheritdoc cref="global::Microsoft.Extensions.Caching.Memory.ICacheEntry"/>
    public interface ICacheEntryCompat : IDisposable
    {
        /// <inheritdoc cref="global::Microsoft.Extensions.Caching.Memory.ICacheEntry.Key"/>
        string Key { get; }

        /// <inheritdoc cref="global::Microsoft.Extensions.Caching.Memory.ICacheEntry.AbsoluteExpiration"/>
        DateTimeOffset? AbsoluteExpiration { get; set; }

        /// <inheritdoc cref="global::Microsoft.Extensions.Caching.Memory.ICacheEntry.AbsoluteExpirationRelativeToNow"/>
        TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        /// <inheritdoc cref="global::Microsoft.Extensions.Caching.Memory.ICacheEntry.SlidingExpiration"/>
        TimeSpan? SlidingExpiration { get; set; }
    }
}

file sealed class CacheEntryCompatImpl : DistributedCacheEntryOptions, CacheExtensions.ICacheEntryCompat, IDisposable
{
    public required string Key { init; get; }

    public void Dispose()
    {
        // 不释放任何资源，由业务方调用 IDistributedCache.Remove 移除缓存值
    }
}