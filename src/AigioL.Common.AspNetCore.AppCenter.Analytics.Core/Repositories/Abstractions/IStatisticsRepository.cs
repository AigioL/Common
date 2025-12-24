using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.Statistics;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus.Summaries;
using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.Abstractions;

public partial interface IStatisticsRepository
{
}

partial interface IStatisticsRepository
{
    /// <summary>
    /// 获取用户相关的统计数据
    /// </summary>
    Task<StatisticsPieResponse[]> GetUserStatisticsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取注册用户相关统计数据
    /// </summary>
    Task<StatisticsLineResponse[]> GetRegisterUserStatisticsAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取活跃用户相关统计数据
    /// </summary>
    Task<ActiveUserSumResponse[]> GetActiveUserStatisticsAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取活跃相关统计数据
    /// </summary>
    Task<StatisticsActiveUserOSResponse[]> GetActiveStatisticsAsync(
#pragma warning disable CS0618 // 类型或成员已过时
        WebApiCompatDevicePlatform? platform,
#pragma warning restore CS0618 // 类型或成员已过时
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取活跃系统分布情况
    /// </summary>
    Task<StatisticsChartActiveUserOSResponse[]> GetOsVersionStatisticsAsync(
#pragma warning disable CS0618 // 类型或成员已过时
        WebApiCompatDevicePlatform? platform,
#pragma warning restore CS0618 // 类型或成员已过时
        DateTimeOffset time,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取广告相关统计数据
    /// </summary>
    Task<StatisticsKomaasharuResponse[]> GetAdvertisementStatisticsAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取活跃相关统计数据
    /// </summary>
    Task<UserActivityStatisticsResponse[]> GetUserActivityStatisticsAsync(
#pragma warning disable CS0618 // 类型或成员已过时
        WebApiCompatDevicePlatform? platform,
#pragma warning restore CS0618 // 类型或成员已过时
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);

    ///// <summary>
    ///// 获取短信使用趋势统计
    ///// </summary>
    //Task<StatisticsSmsUsageTrendResponse[]> GetSmsUsageTrendStatisticsAsync(
    //    DateTimeOffset startTime,
    //    DateTimeOffset endTime,
    //    CancellationToken cancellationToken = default);
}