using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Constants;
using MemoryPack;
using StackExchange.Redis;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Jobs;

/// <summary>
/// 更新 AppVer 缓存任务
/// </summary>
public sealed partial class AppVerRedisCacheJob(
    IAppVerRepository appVerRepository,
    IConnectionMultiplexer redisConnection,
    ILogger<AppVerRedisCacheJob> logger,
    AppDbContext dbContext,
    IFeishuApiClient feishuApiClient) : JobService<AppDbContext, AppVerRedisCacheJob>(logger, dbContext, feishuApiClient)
{
    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
    {
        var redisDb = redisConnection.GetDatabase(CacheKeys.RedisHashDataDb);
        var appVers = await appVerRepository.GetAppVerAllAsync();
        var appVerKeys = await redisDb.HashKeysAsync(CacheKeys.AppVersionHashKey);
        var removeKeys = appVerKeys.Select(x => x.ToString()).Except(appVers.Select(x => ShortGuid.Encode(x.Id)));

        // 删除数据库中不存在的缓存
        foreach (var item in removeKeys)
        {
            await redisDb.HashDeleteAsync(CacheKeys.AppVersionHashKey, item);
        }
        foreach (var item in appVers)
        {
            var idS = ShortGuid.Encode(item.Id);
            await redisDb.HashSetAsync(CacheKeys.AppVersionHashKey, idS, MemoryPackSerializer.Serialize(new AppVerRedisModel
            {
                CreationTime = item.CreationTime,
                Id = item.Id,
                Disable = item.Disable,
                PrivateKey = item.PrivateKey,
                Version = item.Version,
            }));
            await redisDb.HashSetAsync(CacheKeys.AppVersionHashKey, item.Version, MemoryPackSerializer.Serialize(new AppVerRedisModel
            {
                CreationTime = item.CreationTime,
                Id = item.Id,
                Disable = item.Disable,
                PrivateKey = item.PrivateKey,
                Version = item.Version,
            }));
        }
        var lastItem = appVers.OrderByDescending(x => x.CreationTime).FirstOrDefault();
        if (lastItem != null)
        {
            await redisDb.HashSetAsync(CacheKeys.AppVersionHashKey, "GetLastOrDefaultAsync", MemoryPackSerializer.Serialize(new AppVerRedisModel
            {
                CreationTime = lastItem.CreationTime,
                Id = lastItem.Id,
                Disable = lastItem.Disable,
                PrivateKey = lastItem.PrivateKey,
                Version = lastItem.Version,
            }));
        }
        return true;
    }
}
