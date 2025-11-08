using StackExchange.Redis;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Jobs;

/// <summary>
/// 更新 AppVer 缓存任务
/// </summary>
public sealed partial class AppVerRedisCacheJob(
    //IAppVerRepository appVerRepository,
    IConnectionMultiplexer redisConnection,
    ILogger<AppVerRedisCacheJob> logger,
    AppDbContext dbContext,
    IFeishuApiClient feishuApiClient) : JobService<AppDbContext, AppVerRedisCacheJob>(logger, dbContext, feishuApiClient)
{
    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
    {
        // TODO: 实现 AppVer 缓存更新逻辑

        return true;
    }
}
