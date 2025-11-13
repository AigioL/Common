namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Jobs;

/// <summary>
/// 日活统计汇总任务
/// </summary>
sealed partial class DailyStatisticsJob(
    ILogger<DailyStatisticsJob> logger,
    AppDbContext dbContext,
    IFeishuApiClient feishuApiClient) : JobService<AppDbContext, DailyStatisticsJob>(logger, dbContext, feishuApiClient)
{
    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
    {
        // TODO: 待实现

        return true;
    }
}

