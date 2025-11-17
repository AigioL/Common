using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Repositories.Komaasharus.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Jobs;

public sealed partial class KomaasharuCacheJob(
    IKomaasharuRepository komaasharuRepo,
    IDistributedCache distributedCache,
    IConnectionMultiplexer redisConnection,
    ILogger<KomaasharuCacheJob> logger,
    AppDbContext dbContext,
    IFeishuApiClient feishuApiClient) : JobService<AppDbContext, KomaasharuCacheJob>(logger, dbContext, feishuApiClient)
{

    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
    {
        #region CacheModels

        await SetAdvertisementCache(distributedCache, komaasharuRepo);

        #endregion CacheModels

        #region PersonalizedCache 个性化推荐广告

        //var allPersonalized = (await advertisementPersonalizedRepo.GetAllPersonalizedCacheAsync()).ToArray();
        //await cache.SetAsync(CacheKeys.AdvertisementPersonalizedCacheKey, allPersonalized, 60);

        #endregion PersonalizedCache 个性化推荐广告

        #region Statistic 统计

        var dbConnection = redisConnection.GetDatabase(CacheKeys.RedisHashIncrementDb);
        var jumpKeys = await dbConnection.HashGetAllAsync(CacheKeys.AdvertisementJumpHashKey);
        var statisticDatas = new List<KomaasharuStatisticData>();

        // 点击次数
        foreach (var item in jumpKeys)
        {
            string? itemName = item.Name;
            if (Guid.TryParse(itemName, out Guid id))
            {
                var jumpCount = (long)item.Value;
                if (jumpCount > 0)
                {
                    // 减去查询后的次数
                    await dbConnection.HashDecrementAsync(CacheKeys.AdvertisementJumpHashKey, item.Name, (long)item.Value);
                    statisticDatas.Add(new()
                    {
                        Id = id,
                        TotalClick = jumpCount,
                    });
                }
            }
        }

        // 图片（展示/查询）次数
        var imagesKeys = await dbConnection.HashGetAllAsync(CacheKeys.AdvertisementImagesHashKey);
        foreach (var item in imagesKeys)
        {
            string? itemName = item.Name;
            if (Guid.TryParse(itemName, out Guid id))
            {
                var imageCount = (long)item.Value;
                if (imageCount > 0)
                {
                    // 减去查询后的次数
                    await dbConnection.HashDecrementAsync(CacheKeys.AdvertisementImagesHashKey, item.Name, (long)item.Value);
                    var info = statisticDatas.FirstOrDefault(x => x.Id == id);
                    if (info != default)
                    {
                        info.TotalDisplay = imageCount;
                    }
                    else
                    {
                        statisticDatas.Add(new()
                        {
                            Id = id,
                            TotalDisplay = imageCount,
                        });
                    }
                }
            }
        }

        // 保存全部数据到数据库
        foreach (var item in statisticDatas)
        {
            if (item.TotalDisplay > 0 || item.TotalClick > 0)
            {
                await komaasharuRepo.AddCounterAsync(item.Id, item.TotalDisplay, item.TotalClick);
            }
        }

        #endregion Statistic 统计

        return true;
    }

    /// <summary>
    /// 广告数据缓存过期时间 60 分钟
    /// </summary>
    const int advertisement_timeout_minutes = 60;

    /// <summary>
    /// 设置广告数据缓存
    /// </summary>
    static async Task SetAdvertisementCache(
        IDistributedCache cache,
        IKomaasharuRepository komaasharuRepo,
        int timeout_minutes = advertisement_timeout_minutes)
    {
        var models = await komaasharuRepo.GetCacheModelsAsync();
        await cache.SetV2Async(CacheKeys.AdvertisementCacheKey, models, timeout_minutes);
        foreach (var it in models)
        {
            var idString = it.Id.ToString();
            await cache.SetV2Async(idString, it, timeout_minutes);
        }
    }
}

file sealed record class KomaasharuStatisticData
{
    public Guid Id { get; set; }

    public long TotalClick { get; set; }

    public long TotalDisplay { get; set; }
}