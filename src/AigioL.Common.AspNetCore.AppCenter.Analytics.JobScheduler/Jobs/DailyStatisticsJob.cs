using AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Constants;
using StackExchange.Redis;
using static AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.Abstractions.IStatisticsRepository;

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
        var statisticsRepo = serviceProvider.GetRequiredService<IStatisticsRepository>();
        var firstRecord = await statisticsRepo.GetFirstRecord(cancellationToken);
        if (firstRecord != null)
        {
            logger.LogInformation("每日活跃用户汇总 Job 开始");
            var start = ToUTC8Date(firstRecord.CreateTime);
            TimeSpan sp = ToUTC8Date(DateTimeOffset.Now).Subtract(start);
            logger.LogInformation("活跃用户汇总 Job，准备开始 最早的记录是（{start}-{startAddDays1}）",
                start,
                start.AddDays(1));

            for (int i = 1; i <= sp.Days; i++)
            {
                var now = ToUTC8(DateTimeOffset.Now);
                var end = start.AddDays(1);
                if (end >= now)
                {
                    logger.LogInformation("活跃用户汇总 Job，{end} 结束时间过早跳过", end);
                    return;
                }
                logger.LogInformation("统计用户活跃数据开始：{now}（{start}-{end}）",
                    now,
                    start,
                    end);

                var allCount = await statisticsRepo.StatisticsDaysData(start, end);
                if (allCount == 0)
                {
                    logger.LogWarning("活跃用户汇总 Job 处理时间 {start} - {end} 计数为零！将在下次继续重试，此次运行跳过",
                        start,
                        end);
                }
                logger.LogInformation($"统计用户活跃数据结束：{now}", $"（{start}-{end}）",
                    now,
                    start,
                    end);
                start = end;
            }

            // 只保留近两个月的数据
            var delStart = DateTimeOffset.MinValue;
            var delEnd = ToUTC8Date(DateTimeOffset.Now).AddMonths(-2);
            var deleteCount = await statisticsRepo.DeleteDaysData(
                delStart,
                delEnd);
            if (deleteCount > 0)
            {
                logger.LogInformation("统计用户活跃数据删除（实际删除数为 {deleteCount}）", deleteCount);
            }
        }
    }

    /// <summary>
    /// 统计每日版本信息
    /// </summary>
    static async ValueTask ActiveUserAppVerStatisticsDaily(
        ILogger logger,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        var statisticsRepo = serviceProvider.GetRequiredService<IStatisticsRepository>();
        var firstRecord = await statisticsRepo.GetFirstRecord(cancellationToken);
        if (firstRecord != null)
        {
            logger.LogInformation("每日活跃用户版本汇总 Job 开始");
            var start = ToUTC8Date(firstRecord.CreateTime);
            TimeSpan sp = ToUTC8Date(DateTimeOffset.Now).Subtract(start);

            for (int i = 1; i <= sp.Days; i++)
            {
                var now = ToUTC8(DateTimeOffset.Now);
                var end = start.AddDays(1);
                if (end >= now)
                {
                    return;
                }
                var allCount = await statisticsRepo.StatisticsAppVerDaysData(start, end);
                start = end;
            }
        }
    }

    /// <summary>
    /// 统计每日广告信息
    /// </summary>
    static async ValueTask AdvertisementStatisticsDaily(
        ILogger logger,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        const string JOB_NAME = "统计每日广告信息 Job";

        var statisticsRepo = serviceProvider.GetRequiredService<IStatisticsRepository>();
        var dateStart = await GetStartDateOfAdvertisementStatisticsDaily(statisticsRepo, cancellationToken);
        var dateEnd = dateStart != null ? ToUTC8Date(DateTimeOffset.Now) : (DateTimeOffset?)null;
        if (dateStart == null || dateEnd == null)
        {
            logger.LogWarning($"{JOB_NAME}：未能获取到合适的统计开始日期");
            return;
        }

        for (var oneDay = dateStart.Value; oneDay < dateEnd; oneDay = oneDay.AddDays(1))
        {
            var logPrefix = $"{JOB_NAME}：统计日期 {oneDay.Date:d}";

            logger.LogInformation("{logPrefix} 开始", logPrefix);
            try
            {
                var allCount = await statisticsRepo.PerformAdvertisementStatisticDaily(oneDay);
                if (allCount == 0)
                {
                    logger.LogWarning("{logPrefix} 计数为零！将在下次继续重试，此次运行跳过", logPrefix);
                }
            }
            finally
            {
                logger.LogInformation("{logPrefix} 结束", logPrefix);
            }
        }
    }

    static async Task<DateTimeOffset?> GetStartDateOfAdvertisementStatisticsDaily(
        IStatisticsRepository statisticsRepo,
        CancellationToken cancellationToken = default)
    {
        // 取上次日统计日期 加上一天
        var lastDayStatistic = await statisticsRepo.GetLastAdvertisementStatisticDaily(cancellationToken);
        if (lastDayStatistic != null)
        {
            return ToUTC8Date(lastDayStatistic.StatisticsTime).AddDays(1);
        }

        // 取第一条广告统计日期
        var firstStatistic = await statisticsRepo.GetFirstAdvertisementStatistic(cancellationToken);
        if (firstStatistic != null)
        {
            return ToUTC8Date(firstStatistic.CreateTime);
        }

        return null;
    }

    /// <summary>
    /// 统计用户日、周、月活跃度
    /// </summary>
    static async ValueTask ActiveUserStatisticsDWMDaily(
        ILogger logger,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        const string JOB_NAME = "统计用户日、周、月活跃度 Job";

        var statisticsRepo = serviceProvider.GetRequiredService<IStatisticsRepository>();
        var dataStart = await GetStartDateOfUserDWMStatisticDaily(statisticsRepo, cancellationToken);
        var dateEnd = dataStart != null ? ToUTC8Date(DateTimeOffset.Now) : (DateTimeOffset?)null;

        if (dataStart == null || dateEnd == null)
        {
            logger.LogWarning($"{JOB_NAME}：未能获取到合适的统计开始日期");
            return;
        }

        for (var oneDay = dataStart.Value; oneDay < dateEnd; oneDay = oneDay.AddDays(1))
        {
            string logPrefix = $"{JOB_NAME}：统计日期 {oneDay.Date:d}";

            logger.LogInformation("{logPrefix} 开始", logPrefix);
            try
            {
                var allCount = await statisticsRepo.PerformUserDWMStatisticDaily(oneDay);
                if (allCount == 0)
                {
                    logger.LogWarning("{logPrefix} 计数为零！将在下次继续重试，此次运行跳过", logPrefix);
                }
            }
            finally
            {
                logger.LogInformation("{logPrefix} 结束", logPrefix);
            }
        }
    }

    static async Task<DateTimeOffset?> GetStartDateOfUserDWMStatisticDaily(
        IStatisticsRepository statisticsRepo,
        CancellationToken cancellationToken = default)
    {
        // 取上次日统计日期 加上一天
        var lastDayStatistic = await statisticsRepo.GetLastUserDWMStatisticDaily(cancellationToken);
        if (lastDayStatistic != null)
        {
            return ToUTC8Date(lastDayStatistic.StatisticsTime).AddDays(1);
        }

        // 取上月月初 1 号日期
        var nowDate = ToUTC8Date(DateTimeOffset.Now);
        return nowDate.AddDays(-nowDate.Day + 1).AddMonths(-1);
    }

    /// <summary>
    /// 统计订单金额和数量
    /// </summary>
    static async ValueTask OrderAmountQtyStatisticsDaily(
        ILogger logger,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        const string JOB_NAME = "统计订单金额和数量 Job";

        var statisticsRepo = serviceProvider.GetRequiredService<IStatisticsRepository>();
        var dateSta = await GetStartDateOfOrderStatisticDaily(statisticsRepo, cancellationToken);
        var dateEnd = dateSta != null ? DateTimeOffset.Now.Date : (DateTimeOffset?)null;

        if (dateSta == null || dateEnd == null)
        {
            logger.LogWarning($"{JOB_NAME}：未能获取到合适的统计开始日期");
            return;
        }

        for (var oneDay = dateSta.Value; oneDay < dateEnd; oneDay = oneDay.AddDays(1))
        {
            string logPrefix = $"{JOB_NAME}：统计日期 {oneDay.Date:d}";

            logger.LogInformation("{logPrefix} 开始", logPrefix);
            try
            {
                var allCount = await statisticsRepo.PerformOrderAmountQtyStatisticDaily(oneDay);
                if (allCount == 0)
                {
                    logger.LogWarning("{logPrefix} 计数为零！将在下次继续重试，此次运行跳过", logPrefix);
                }
            }
            finally
            {
                logger.LogInformation("{logPrefix} 结束", logPrefix);
            }
        }
    }

    static async Task<DateTimeOffset?> GetStartDateOfOrderStatisticDaily(
        IStatisticsRepository statisticsRepo,
        CancellationToken cancellationToken = default)
    {
        // 取上次日统计日期 加上一天
        var lastDayStatistic = await statisticsRepo.GetLastOrderAmountQtySummary(cancellationToken);
        if (lastDayStatistic != null)
        {
            return ToUTC8Date(lastDayStatistic.StatisticsTime).AddDays(1);
        }

        // 取上月月初 1 号日期
        var nowDate = ToUTC8Date(DateTimeOffset.Now);
        return nowDate.AddDays(-nowDate.Day + 1).AddMonths(-1);
    }
}
