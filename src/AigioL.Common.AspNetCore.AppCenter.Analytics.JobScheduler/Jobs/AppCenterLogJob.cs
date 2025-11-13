namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Jobs;

/// <summary>
/// AppCenter 统计汇总任务
/// </summary>
sealed partial class AppCenterLogJob(
    ILogger<AppCenterLogJob> logger,
    AppDbContext dbContext,
    IFeishuApiClient feishuApiClient) : JobService<AppDbContext, AppCenterLogJob>(logger, dbContext, feishuApiClient)
{
    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
    {
        // TODO: 待实现

        return true;
    }
}
