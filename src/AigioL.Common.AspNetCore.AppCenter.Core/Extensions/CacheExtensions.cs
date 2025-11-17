using MemoryPack;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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
                    try
                    {
                        var logger = Log.CreateLogger(nameof(CacheExtensions));
                        var tType = typeof(TItem);
                        var typeName = tType.FullName ?? tType.Name;
                        LogErrorByGetV2Async(logger, typeName);
                    }
                    catch
                    {
                    }
                    // 反序列化失败
                    throw;
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

    /// <summary>
    /// 异步从缓存中获取指定键的值
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T?> GetV2Async<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        this IDistributedCache cache,
        string key,
        CancellationToken cancellationToken = default) where T : notnull
    {
        T? value = default;
        var bytes = await cache.GetAsync(key, cancellationToken);
        if (bytes != null)
        {
            try
            {
                value = MemoryPackSerializer.Deserialize<T>(bytes);
            }
            catch
            {
                try
                {
                    var logger = Log.CreateLogger(nameof(CacheExtensions));
                    var tType = typeof(T);
                    var typeName = tType.FullName ?? tType.Name;
                    LogErrorByGetV2Async(logger, typeName);
                }
                catch
                {
                }
                // 反序列化失败
                throw;
            }
        }
        return value;
    }

    /// <summary>
    /// 异步从缓存中获取指定键的值
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<(byte[]? bytes, T? t)> GetV2WithBinaryAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        this IDistributedCache cache,
        string key,
        CancellationToken cancellationToken = default) where T : notnull
    {
        T? value = default;
        var bytes = await cache.GetAsync(key, cancellationToken);
        if (bytes != null)
        {
            try
            {
                value = MemoryPackSerializer.Deserialize<T>(bytes);
            }
            catch
            {
                try
                {
                    var logger = Log.CreateLogger(nameof(CacheExtensions));
                    var tType = typeof(T);
                    var typeName = tType.FullName ?? tType.Name;
                    LogErrorByGetV2Async(logger, typeName);
                }
                catch
                {
                }
                // 反序列化失败
                throw;
            }
        }
        return (bytes, value);
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "异步从缓存中获取指定键的值时出错，类型：{typeName}")]
    private static partial void LogErrorByGetV2Async(ILogger logger, string typeName);

    /// <summary>
    /// 异步将指定键和值添加到缓存中，并指定缓存选项
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task SetV2Async<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        this IDistributedCache cache,
        string key,
        T value,
        DistributedCacheEntryOptions options,
        CancellationToken cancellationToken = default) where T : notnull
    {
        var bytes = MemoryPackSerializer.Serialize(value);
        await cache.SetAsync(key, bytes, options, cancellationToken);
    }

    /// <summary>
    /// 异步指定键和值添加到缓存中，并设置相对于当前时间的绝对过期时间
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task SetV2Async<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        this IDistributedCache cache,
        string key,
        T value,
        TimeSpan absoluteExpirationRelativeToNow,
        CancellationToken cancellationToken = default) where T : notnull
        => cache.SetV2Async(key, value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow,
        }, cancellationToken);

    /// <summary>
    /// 异步将指定键和值添加到分布式缓存中，并设置过期时间
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task SetV2Async<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        this IDistributedCache cache,
        string key,
        T value,
        int minutes,
        CancellationToken cancellationToken = default) where T : notnull
        => cache.SetV2Async(key, value, TimeSpan.FromMinutes(minutes), cancellationToken);
}

file sealed class CacheEntryCompatImpl : DistributedCacheEntryOptions, CacheExtensions.ICacheEntryCompat, IDisposable
{
    public required string Key { init; get; }

    public void Dispose()
    {
        // 不释放任何资源，由业务方调用 IDistributedCache.Remove 移除缓存值
    }
}