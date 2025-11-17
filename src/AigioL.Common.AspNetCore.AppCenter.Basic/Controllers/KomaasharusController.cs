using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Controllers;

/// <summary>
/// 客户端广告 コマーシャル
/// </summary>
public static partial class KomaasharusController
{
    /// <summary>
    /// 广告数据内存中缓存过期时间 1 分钟
    /// </summary>
    const int advertisement_memory_timeout_minutes = 1;

    public static void MapBasicKomaasharus<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "basic/komaasharu")
        where TAppSettings : class, IAppSettings
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapGet("images/{id}", async (HttpContext context, [FromRoute] string id) =>
        {
            string? redirectUrl = null;
            if (!string.IsNullOrWhiteSpace(id) && ShortGuid.TryParse(id, out Guid idG) && idG != default)
            {
                var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
                var memoryCache = context.RequestServices.GetRequiredService<IMemoryCache>();
                var connection = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                redirectUrl = await GetAdvertisementImageUrl(cache, memoryCache, connection, idG, context.RequestAborted);
            }
            var httpsOnly = context.Request.IsHttps;
            return redirectUrl.IsHttpUrl(httpsOnly) ?
                Results.Redirect(redirectUrl) :
                Results.NotFound();
        }).WithDescription("广告图片计数跳转")
            .Produces(StatusCodes.Status302Found)
            .Produces(StatusCodes.Status404NotFound);
        routeGroup.MapGet("{id}", async (HttpContext context, [FromRoute] string id) =>
        {
            string? redirectUrl = null;
            if (!string.IsNullOrWhiteSpace(id) && ShortGuid.TryParse(id, out Guid idG) && idG != default)
            {
                var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
                var memoryCache = context.RequestServices.GetRequiredService<IMemoryCache>();
                var connection = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                redirectUrl = await GetAdvertisementJumpUrl(cache, memoryCache, connection, idG, context.RequestAborted);
            }
            var httpsOnly = context.Request.IsHttps;
            if (!redirectUrl.IsHttpUrl(httpsOnly))
            {
                var settings = context.RequestServices.GetRequiredService<IOptions<TAppSettings>>().Value;
                redirectUrl = settings.OfficialWebsite;
            }
            return redirectUrl.IsHttpUrl(httpsOnly) ?
                Results.Redirect(redirectUrl) :
                Results.NotFound();
        }).WithDescription("广告点击链接🔗")
            .Produces(StatusCodes.Status302Found)
            .Produces(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// 获取广告图片
    /// </summary>
    static async Task<string?> GetAdvertisementImageUrl(
        IDistributedCache cache,
        IMemoryCache memoryCache,
        IConnectionMultiplexer connection,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var idString = id.ToString();
        var key = CacheKeys.GetMethodCacheKey(nameof(GetAdvertisementImageUrl), id);
        var imageUrl = await memoryCache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(advertisement_memory_timeout_minutes);
            var advertisementCache = await cache.GetV2Async<KomaasharuRedisModel>(idString, cancellationToken: cancellationToken);
            return advertisementCache?.ImageUrl;
        });
        if (imageUrl != default)
        {
            var dbConnection = connection.GetDatabase(CacheKeys.RedisHashIncrementDb);
            await dbConnection.HashIncrementAsync(CacheKeys.AdvertisementImagesHashKey, idString);
            return imageUrl;
        }
        return default;
    }

    /// <summary>
    /// 广告点击链接🔗
    /// </summary>
    static async Task<string?> GetAdvertisementJumpUrl(
        IDistributedCache cache,
        IMemoryCache memoryCache,
        IConnectionMultiplexer connection,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var idString = id.ToString();
        var key = CacheKeys.GetMethodCacheKey(nameof(GetAdvertisementJumpUrl), id);
        var jumpUrl = await memoryCache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(advertisement_memory_timeout_minutes);

            var advertisementCache = await cache.GetV2Async<KomaasharuRedisModel>(idString, cancellationToken: cancellationToken);
            return advertisementCache?.JumpUrl;
        });
        if (jumpUrl != default)
        {
            var dbConnection = connection.GetDatabase(CacheKeys.RedisHashIncrementDb, cancellationToken);
            await dbConnection.HashIncrementAsync(CacheKeys.AdvertisementJumpHashKey, idString);
            return jumpUrl;
        }
        return default;
    }
}
