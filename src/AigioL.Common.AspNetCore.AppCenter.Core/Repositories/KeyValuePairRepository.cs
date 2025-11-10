using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using MemoryPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using static AigioL.Common.AspNetCore.AppCenter.Services.Abstractions.IKeyValuePairRepository;
using KeyValuePair = AigioL.Common.AspNetCore.AppCenter.Entities.KeyValuePair;

namespace AigioL.Common.AspNetCore.AppCenter.Services;

public sealed partial class KeyValuePairRepository<TDbContext>(TDbContext dbContext, IServiceProvider serviceProvider) :
#pragma warning disable CS9107 // 参数捕获到封闭类型状态，其值也传递给基构造函数。该值也可能由基类捕获。
    Repository<TDbContext, KeyValuePair, string>(dbContext, serviceProvider),
#pragma warning restore CS9107 // 参数捕获到封闭类型状态，其值也传递给基构造函数。该值也可能由基类捕获。
    IKeyValuePairRepository
    where TDbContext : DbContext, IDbContextBase
{
    public async Task<KeyValuePair?> QueryAsync(string id, CancellationToken cancellationToken = default)
    {
        var r = await EntityNoTracking.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return r;
    }

    public async Task<(ViewLayoutModel? m, string langKey)> GetViewLayoutModelAsync(CancellationToken cancellationToken = default)
    {
        var ls = serviceProvider.GetService<ILocalizationService>();
        var langKey = ls?.GetLangKey();
        if (string.IsNullOrWhiteSpace(langKey))
        {
            langKey = CultureInfo.CurrentUICulture.Name;
        }

        var cache = serviceProvider.GetRequiredService<IDistributedCache>();
        var cacheKey = $"ViewLayoutModel_{langKey}";
        var result = await GetAsync(cache, cacheKey, KeyValuePairJsonSerializerContext.Default.ViewLayoutModel, cancellationToken);
        if (result != null)
        {
            return new(result, langKey);
        }
        cacheKey = "ViewLayoutModel";
        result = await GetAsync(cache, cacheKey, KeyValuePairJsonSerializerContext.Default.ViewLayoutModel, cancellationToken);

        var defLangKey = ls?.GetDefaultLangKey();
        if (string.IsNullOrWhiteSpace(defLangKey))
        {
            defLangKey = "zh-CN";
        }
        return new(result, defLangKey); // 默认语言
    }

    public async Task SetViewLayoutModelAsync(ViewLayoutModel m, string? langKey = null, int? expirationFromMinutes = null)
    {
        string cacheKey;
        if (string.IsNullOrWhiteSpace(langKey))
        {
            cacheKey = "ViewLayoutModel";
        }
        else
        {
            cacheKey = $"ViewLayoutModel_{langKey}";
        }

        var cache = serviceProvider.GetService<IDistributedCache>();
        await SetAsync(cacheKey, m, cache,
            TimeSpan.FromMinutes(expirationFromMinutes ?? KeyExpirationFromMinutes_ViewLayout),
            KeyValuePairJsonSerializerContext.Default.ViewLayoutModel);
    }
}

partial class KeyValuePairRepository<TDbContext>
{
    /// <summary>
    /// 根据键获取值，优先从缓存获取，缓存值使用 MemoryPack 序列化，数据库值使用 System.Text.Json 序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cache"></param>
    /// <param name="key"></param>
    /// <param name="jsonTypeInfo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    async Task<T?> GetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        IDistributedCache cache,
        string key,
        JsonTypeInfo<T> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        T? result = default;
        byte[]? bytes;
        bytes = await cache.GetAsync(key, cancellationToken);
        if (bytes != null && bytes.Length > 0)
        {
            try
            {
                result = MemoryPackSerializer.Deserialize<T>(bytes);
                return result;
            }
            catch
            {
            }
        }
        if (result is null)
        {
            var entry = await EntityNoTracking.FirstOrDefaultAsync(x => x.Id == key, cancellationToken);
            if (entry != null)
            {
                result = JsonSerializer.Deserialize(entry.Value, jsonTypeInfo);
                if (result != null)
                {
                    bytes = MemoryPackSerializer.Serialize(result);
                    await cache.SetAsync(key, bytes, CancellationToken.None);
                }
                return result;
            }
        }
        return default;
    }

    /// <summary>
    /// 根据键值对设置值到数据库中，并更新缓存，缓存值使用 MemoryPack 序列化，数据库值使用 System.Text.Json 序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="cache"></param>
    /// <param name="options"></param>
    /// <param name="key"></param>
    /// <param name="jsonTypeInfo"></param>
    /// <returns></returns>
    async Task SetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        string key,
        T value,
        IDistributedCache? cache,
        DistributedCacheEntryOptions? options,
        JsonTypeInfo<T> jsonTypeInfo)
    {
        var jsonValue = JsonSerializer.Serialize(value, jsonTypeInfo);
        var entity = await FindAsync(key, CancellationToken.None);
        if (entity == null)
        {
            entity = new()
            {
                Id = key,
                Value = jsonValue,
            };
            await InsertAsync(entity, CancellationToken.None);
        }
        else
        {
            entity.Value = jsonValue;
            await UpdateAsync(entity, CancellationToken.None);
        }

        if (cache != null)
        {
            var bytes = MemoryPackSerializer.Serialize(value);
            await cache.SetAsync(key, bytes, options ?? new(), CancellationToken.None);
        }
    }

    /// <inheritdoc cref="SetAsync{T}(string, T, IDistributedCache?, DistributedCacheEntryOptions?, JsonTypeInfo{T})"/>
    Task SetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        string key,
        T value,
        IDistributedCache? cache,
        TimeSpan absoluteExpirationRelativeToNow,
        JsonTypeInfo<T> jsonTypeInfo)
    {
        DistributedCacheEntryOptions? options;
        if (cache != null)
        {
            options = new()
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow,
            };
        }
        else
        {
            options = null;
        }
        return SetAsync(key, value, cache, options, jsonTypeInfo);
    }
}

[JsonSerializable(typeof(ViewLayoutModel))]
[JsonSourceGenerationOptions]
sealed partial class KeyValuePairJsonSerializerContext : JsonSerializerContext
{
    static KeyValuePairJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new KeyValuePairJsonSerializerContext(o);
    }
}