using StackExchange.Redis;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Jobs;

/// <summary>
/// 活跃用户数据批量插入任务
/// </summary>
public sealed partial class ActiveUserInsertJob(
    //IActiveUserRecordRepository activeUserRecordRepository,
    IConnectionMultiplexer redisConnection,
    ILogger<ActiveUserInsertJob> logger,
    AppDbContext dbContext,
    IFeishuApiClient feishuApiClient) : JobService<AppDbContext, ActiveUserInsertJob>(logger, dbContext, feishuApiClient)
{
    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
    {
        //var redisDb = redisConnection.GetDatabase(CacheKeys.RedisHashDataDb);

        //var listLength = await redisDb.ListLengthAsync(nameof(ActiveUserAnonymousStatisticCacheModel));
        //var activeUserDTOs = await redisDb.ListRangeAsync(nameof(ActiveUserAnonymousStatisticCacheModel), 0, listLength);
        //var insertItem = new List<ActiveUserAnonymousStatistic>();
        //foreach (var item in activeUserDTOs)
        //{
        //    var record = MemoryPackSerializer.Deserialize<ActiveUserAnonymousStatisticCacheModel>((byte[])item!);
        //    if (record is not null)
        //    {
        //        insertItem.Add(new ActiveUserAnonymousStatistic
        //        {
        //            Type = record.Type,
        //            IPAddress = record.IPAddress,
        //            Platform = record.Platform,
        //            DeviceIdiom = record.DeviceIdiom,
        //            ProcessArch = record.ProcessArch,
        //            OSVersion = record.OSVersion,
        //            AppVersion = record.AppVersion,
        //            ScreenCount = record.ScreenCount,
        //            PrimaryScreenPixelDensity = record.PrimaryScreenPixelDensity,
        //            PrimaryScreenWidth = record.PrimaryScreenWidth,
        //            PrimaryScreenHeight = record.PrimaryScreenHeight,
        //            SumScreenWidth = record.SumScreenWidth,
        //            SumScreenHeight = record.SumScreenHeight,
        //            IsAuthenticated = record.IsAuthenticated,
        //            OSName = record.OSName,
        //            DeviceId = record.DeviceId,
        //        });
        //    }
        //    else
        //    {
        //        logger.LogError("ActiveUser_V1 缓存活跃用户数据出错 无法序列化");
        //    }
        //}

        //await activeUserRecordRepository.InsertRangeAsync(insertItem);
        //await redisDb.ListTrimAsync(nameof(ActiveUserAnonymousStatisticCacheModel), activeUserDTOs.Length, -1);

        return true;
    }
}