using AigioL.Common.AspNetCore.AppCenter.Basic.Models;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MemoryPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using static AigioL.Common.AspNetCore.AppCenter.Services.Abstractions.IKeyValuePairRepository;
using KeyValuePair = AigioL.Common.AspNetCore.AppCenter.Entities.KeyValuePair;

namespace AigioL.Common.AspNetCore.AppCenter.Services;

public sealed partial class KeyValuePairRepository<TDbContext>(
    TDbContext dbContext,
    IServiceProvider serviceProvider) :
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

    public async Task<string?> QueryValueAsync(string id, CancellationToken cancellationToken = default)
    {
        var r = await EntityNoTracking.Where(x => x.Id == id).Select(x => x.Value).FirstOrDefaultAsync(cancellationToken);
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

partial class KeyValuePairRepository<TDbContext> // 管理后台
{
    public async Task<PagedModel<KeyValuePairTableItemModel>> QueryAsync(
        string? id,
        string? value,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        IQueryable<KeyValuePair> query = EntityNoTracking
            .OrderBy(static x => x.CreateTime);

        if (!string.IsNullOrWhiteSpace(id))
        {
            query = query.Where(x => x.Id != null && x.Id == id);
        }
        if (!string.IsNullOrWhiteSpace(value))
        {
            query = query.Where(x => x.Value == value);
        }

        var query2 = query.ProjectTo<KeyValuePairTableItemModel>(mapper.ConfigurationProvider);

        var r = await query2.PagingAsync(current, pageSize, cancellationToken);
        return r;
    }

    public async Task<bool> UpdateAsync(
        Guid? operatorUserId,
        AddOrEditKeyValuePairModel model,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            return false;
        }

        var entity = await Entity
            .IgnoreQueryFilters()
            .Where(x => x.Id == model.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity == null)
        {
            return false;
        }

        var connection = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
        var cache = serviceProvider.GetRequiredService<IDistributedCache>();

        entity.Value = model.Value;
        entity.OperatorUserId = operatorUserId;
        entity.DeleteTime = null;
        await db.SaveChangesAsync(CancellationToken.None);

        // 清除内存缓存
        await ClearCacheAsync(connection, cache, model.Id);

        return true;
    }

    public async Task<bool> InsertAsync(
        Guid? createUserId,
        AddOrEditKeyValuePairModel model,
        CancellationToken cancellationToken = default)
    {
        var connection = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
        var cache = serviceProvider.GetRequiredService<IDistributedCache>();
        var entity = await Entity
            .IgnoreQueryFilters()
            .Where(x => x.Id == model.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity == null)
        {
            entity = new()
            {
                Id = model.Id, // 非自增主键使用传递的值
                Value = model.Value,
                CreateUserId = createUserId,
            };
            await db.AddAsync(entity, CancellationToken.None);
        }
        else
        {
            entity.Value = model.Value;
            entity.OperatorUserId = createUserId;
            entity.DeleteTime = null;
        }
        await db.SaveChangesAsync(CancellationToken.None);

        // 清除内存缓存
        await ClearCacheAsync(connection, cache, model.Id);

        return true;
    }

    public async Task<int> DeleteAsync(
        string primaryKey,
        Guid? operatorUserId = null)
    {
        var connection = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
        var cache = serviceProvider.GetRequiredService<IDistributedCache>();
        var r = await base.DeleteAsync(primaryKey, operatorUserId, CancellationToken.None);

        // 清除内存缓存
        await ClearCacheAsync(connection, cache, primaryKey);

        return r;
    }

    public sealed override Task<int> DeleteAsync(
        string primaryKey,
        Guid? operatorUserId = null,
        CancellationToken cancellationToken = default) => DeleteAsync(primaryKey, operatorUserId);

    public async Task<int> PhysicalDeleteAsync(
        string primaryKey)
    {
        var connection = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
        var cache = serviceProvider.GetRequiredService<IDistributedCache>();

        var query = Entity
            .IgnoreQueryFilters()
            .Where(x => x.Id == primaryKey);
        var r = await query.ExecuteDeleteAsync();

        // 清除内存缓存
        await ClearCacheAsync(connection, cache, primaryKey);

        return r;
    }

    public async Task<int> SwitchAsync(
        string primaryKey,
        Guid? operatorUserId,
        bool? enable)
    {
        var connection = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
        var cache = serviceProvider.GetRequiredService<IDistributedCache>();

        var query = Entity
            .IgnoreQueryFilters()
            .Where(x => x.Id == primaryKey);
        int r;

        if (enable.HasValue)
        {
            r = await query.ExecuteUpdateAsync(x => x
                .SetProperty(y => y.DeleteTime, y => enable.Value ? null : DateTimeOffset.UtcNow)
                .SetProperty(y => y.OperatorUserId, y => operatorUserId)
                );
        }
        else
        {
            r = await query.ExecuteUpdateAsync(x => x
                .SetProperty(y => y.DeleteTime, y => y.DeleteTime == null ? DateTimeOffset.UtcNow : null)
                .SetProperty(y => y.OperatorUserId, y => operatorUserId)
                );
        }

        // 清除内存缓存
        await ClearCacheAsync(connection, cache, primaryKey);

        return r;
    }

    async Task ClearCacheAsync(
        IConnectionMultiplexer connection,
        IDistributedCache cache,
        string key)
    {
        // 清除内存缓存
        await connection.GetDatabase().HashDeleteAsync(CacheKey, key);
        await cache.RemoveAsync(key, CancellationToken.None);
    }
}

partial class KeyValuePairRepository<TDbContext>
{
    public async Task<T?> GetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        IDistributedCache cache,
        string key,
        JsonTypeInfo<T>? jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        bool isStringType = typeof(T) == typeof(string);

        T? result = default;
        byte[]? bytes;
        bytes = await cache.GetAsync(key, cancellationToken);
        if (bytes != null)
        {
            if (bytes.Length == 0)
            {
                // 空数组表示查询数据库无值，返回默认值
                return default;
            }
            else
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
        }
        if (result is null)
        {
            var entry = await EntityNoTracking.FirstOrDefaultAsync(x => x.Id == key, cancellationToken);
            if (entry != null)
            {
                if (isStringType)
                {
                    // 字符串类型直接转换
                    result = (T)(object)entry.Value;
                }
                else
                {
                    // 从数据库中取的值使用 Json 反序列化
                    ArgumentNullException.ThrowIfNull(jsonTypeInfo);
                    result = JsonSerializer.Deserialize(entry.Value, jsonTypeInfo);
                }

                // 从数据库中取的值为 null 时，缓存空数组表示无值
                bytes = result == null ? [] : MemoryPackSerializer.Serialize(result);
                await cache.SetAsync(key, bytes, CancellationToken.None);
                return result;
            }
        }
        return default;
    }

    public async Task SetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
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

    public Task SetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
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