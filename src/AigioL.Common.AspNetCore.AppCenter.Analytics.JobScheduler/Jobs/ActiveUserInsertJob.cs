using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Constants;
using MemoryPack;
using StackExchange.Redis;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Jobs;

/// <summary>
/// 活跃用户数据批量插入任务
/// </summary>
public sealed partial class ActiveUserInsertJob(
    IActiveUserRecordRepository activeUserRecordRepository,
    IConnectionMultiplexer redisConnection,
    ILogger<ActiveUserInsertJob> logger,
    AppDbContext dbContext,
    IFeishuApiClient feishuApiClient) : JobService<AppDbContext, ActiveUserInsertJob>(logger, dbContext, feishuApiClient)
{
    const string k = nameof(ActiveUserAnonymousStatisticCacheModel);

    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
    {
        var redisDb = redisConnection.GetDatabase(CacheKeys.RedisHashDataDb);

        var len = await redisDb.ListLengthAsync(k);
        var redisValues = await redisDb.ListRangeAsync(k, 0, len);
        var entities = new ActiveUserAnonymousStatistic[len];

        int index = 0;
        for (int i = 0; i < len; i++)
        {
            var item = redisValues[i];
            if (item.HasValue)
            {
                var record = MemoryPackSerializer.Deserialize<ActiveUserAnonymousStatisticCacheModel>((byte[])item!);
                if (record != null && !string.IsNullOrWhiteSpace(record.Model.OSVersion))
                {
                    var e = new ActiveUserAnonymousStatistic
                    {
                        Type = record.Model.Type,
                        IPAddress = record.IPAddress,
                        Platform = record.Model.Platform,
                        DeviceIdiom = record.Model.DeviceIdiom,
                        ProcessArch = record.Model.ProcessArch,
                        OSVersion = record.Model.OSVersion,
                        AppVersion = record.AppVersion,
                        ScreenCount = record.Model.ScreenCount,
                        PrimaryScreenPixelDensity = record.Model.PrimaryScreenPixelDensity,
                        PrimaryScreenWidth = record.Model.PrimaryScreenWidth,
                        PrimaryScreenHeight = record.Model.PrimaryScreenHeight,
                        SumScreenWidth = record.Model.SumScreenWidth,
                        SumScreenHeight = record.Model.SumScreenHeight,
                        IsAuthenticated = record.Model.IsAuthenticated,
                        OSName = record.DevicePlatform,
                        DeviceId = record.DeviceId,
                    };
                    entities[index++] = e;
                }
            }
        }

        IEnumerable<ActiveUserAnonymousStatistic> entities_ = index == len ? entities : entities.Take(index);
        await activeUserRecordRepository.InsertRangeAsync(entities_, CancellationToken.None);
        await redisDb.ListTrimAsync(k, len, -1);

        return true;
    }
}