using AigioL.Common.AspNetCore.AppCenter.Constants;
using StackExchange.Redis;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Jobs;

/// <summary>
/// 日活统计汇总任务
/// </summary>
sealed partial class DailyStatisticsJob(
    IServiceProvider serviceProvider,
    ILogger<DailyStatisticsJob> logger,
    AppDbContext dbContext,
    IFeishuApiClient feishuApiClient) : JobService<AppDbContext, DailyStatisticsJob>(logger, dbContext, feishuApiClient)
{
    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
    {
        // 并行化执行每个统计任务
        ParallelTaskItem[] parallelTasks =
        [
            (Delegate)DeleteCacheKey,
            (Delegate)ActiveUserStatisticsDaily,
            (Delegate)ActiveUserStatisticsDWMDaily,
            (Delegate)AdvertisementStatisticsDaily,
            (Delegate)OrderAmountQtyStatisticsDaily,
            (Delegate)ActiveUserAppVerStatisticsDaily,
        ];
        await Parallel.ForEachAsync(parallelTasks, cancellationToken, Invoke);
        return true;
    }

    async ValueTask Invoke(ParallelTaskItem it, CancellationToken cancellationToken = default)
    {
        string? taskName = null;
        Delegate? @delegate = null;

        string GetTaskName()
        {
            if (taskName is not null)
                return taskName;
            if (@delegate is not null)
                return @delegate.Method.Name;
            return "Unknown";
        }

        try
        {
            if (it.TaskFunc != null)
            {
                using var s = serviceProvider.CreateScope();
                await it.TaskFunc(logger, s.ServiceProvider, cancellationToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "{taskName} Job 任务处理出现错误", GetTaskName());
        }
    }

    sealed unsafe class ParallelTaskItem
    {
        public string? TaskName { get; set; }

        public Func<ILogger, IServiceProvider, CancellationToken, ValueTask>? TaskFunc { get; init; }

        public static implicit operator ParallelTaskItem(Delegate func) => new() { TaskFunc = (Func<ILogger, IServiceProvider, CancellationToken, ValueTask>)func };
    }
}

partial class DailyStatisticsJob
{
    static string GetSplitFirstWithTrim(ReadOnlySpan<char> s, char separator)
    {
        var split = s.Split(separator);
        split.MoveNext();
        return new(s[split.Current].Trim());
    }

    /// <summary>
    /// 清理活跃用户设备 Id
    /// </summary>
    static async ValueTask DeleteCacheKey(
        ILogger logger,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        var conn = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
        var hostAndPort = GetSplitFirstWithTrim(conn.Configuration.AsSpan(), ',');
        var server = conn.GetServer(hostAndPort);
        var keys = server.KeysAsync(CacheKeys.RedisActiveUserDb);

        var today = DateTime.Today;
        var todayStr = today.ToString("yyyy-MM-dd");

        var db = conn.GetDatabase(CacheKeys.RedisActiveUserDb);

        await foreach (var it in keys)
        {
            var keyStr = it.ToString();
            if (keyStr.StartsWith(todayStr))
            {
                await db.KeyDeleteAsync(it);
            }
        }
    }

    /// <summary>
    /// 统计每日活跃用户信息
    /// </summary>
    static async ValueTask ActiveUserStatisticsDaily(
        ILogger logger,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {

    }

    /// <summary>
    /// 统计每日版本信息
    /// </summary>
    static async ValueTask ActiveUserAppVerStatisticsDaily(
        ILogger logger,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {

    }

    /// <summary>
    /// 统计每日广告信息
    /// </summary>
    static async ValueTask AdvertisementStatisticsDaily(
        ILogger logger,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {

    }

    /// <summary>
    /// 统计用户日、周、月活跃度
    /// </summary>
    static async ValueTask ActiveUserStatisticsDWMDaily(
        ILogger logger,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {

    }

    /// <summary>
    /// 统计订单金额和数量
    /// </summary>
    static async ValueTask OrderAmountQtyStatisticsDaily(
        ILogger logger,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {

    }
}
