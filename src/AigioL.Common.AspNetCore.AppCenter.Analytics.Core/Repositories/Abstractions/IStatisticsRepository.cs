using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.Statistics;
using AigioL.Common.AspNetCore.AppCenter.Entities.Komaasharus.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
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
        DateOnly startTime,
        DateOnly endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取活跃用户相关统计数据
    /// </summary>
    Task<ActiveUserSumResponse[]> GetActiveUserStatisticsAsync(
        DateOnly startTime,
        DateOnly endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取活跃相关统计数据
    /// </summary>
    Task<StatisticsActiveUserOSResponse[]> GetActiveStatisticsAsync(
#pragma warning disable CS0618 // 类型或成员已过时
        WebApiCompatDevicePlatform? platform,
#pragma warning restore CS0618 // 类型或成员已过时
        DateOnly startTime,
        DateOnly endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取活跃系统分布情况
    /// </summary>
    Task<StatisticsChartActiveUserOSResponse[]> GetOsVersionStatisticsAsync(
#pragma warning disable CS0618 // 类型或成员已过时
        WebApiCompatDevicePlatform? platform,
#pragma warning restore CS0618 // 类型或成员已过时
        DateOnly time,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取广告相关统计数据
    /// </summary>
    Task<StatisticsKomaasharuResponse[]> GetAdvertisementStatisticsAsync(
        DateOnly startTime,
        DateOnly endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取活跃相关统计数据
    /// </summary>
    Task<UserActivityStatisticsResponse[]> GetUserActivityStatisticsAsync(
#pragma warning disable CS0618 // 类型或成员已过时
        WebApiCompatDevicePlatform? platform,
#pragma warning restore CS0618 // 类型或成员已过时
        DateOnly startTime,
        DateOnly endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取短信使用趋势统计
    /// </summary>
    Task<StatisticsSmsUsageTrendResponse[]> GetSmsUsageTrendStatisticsAsync(
        DateOnly startTime,
        DateOnly endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取邮件发送趋势统计
    /// </summary>
    Task<StatisticsEmailUsageTrendResponse[]> GetEmailUsageTrendStatisticsAsync(
        DateOnly startTime,
        DateOnly endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取订单的金额数量统计
    /// </summary>
    Task<StatisticsOrderAmountQtyModel[]> GetOrderAmountQtyStatisticsAsync(
        DateOnly startTime,
        DateOnly endTime,
        string? unit,
        PaymentType? paymentType,
        bool taxed,
        bool pureProfit,
        int businessTypeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取订单的金额数量统计表格
    /// </summary>
    Task<OrderAmountQtyTableModel[]> GetOrderSummaryTable(
        DateOnly startTime,
        DateOnly endTime,
        OrderType[]? orderTypes = null,
        int[]? orderBusinessTypeIds = null,
        string? orderBy = null,
        bool? desc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取启动服务数据
    /// </summary>
    Task<AnalysisResponse[]?> GetStartServiceLogAnalysis(
        DateOnly startTime,
        DateOnly endTime,
        string? appVersion,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取开始会话数据
    /// </summary>
    Task<AnalysisResponse[]?> GetStartSessionLogAnalysis(
        DateOnly startTime,
        DateOnly endTime,
        string? appVersion,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取事件的菜单
    /// </summary>
    Task<string[]> GetAnalysisEvnetMenuList(
        DateOnly startTime,
        DateOnly endTime,
        string? appVersion,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取事件属性菜单
    /// </summary>
    Task<string[]?> GetAnalysisPropertiesKeyMenuList(
        DateOnly startTime,
        DateOnly endTime,
        string? appVersion,
        string? eventNames,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取事件属性 Value 菜单
    /// </summary>
    Task<string[]?> GetAnalysisPropertiesValueMenuList(
        DateOnly startTime,
        DateOnly endTime,
        string? appVersion,
        string? eventNames,
        string? key,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取事件统计数据
    /// </summary>
    Task<AnalysisResponse[]?> GetAnalysisEventSummary(
        DateOnly startTime,
        DateOnly endTime,
        string? appVersion,
        string? eventNames,
        string? key,
        string? value,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取版本数据
    /// </summary>
    Task<string[]?> GetAnalysisAppVer(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取版本使用趋势
    /// </summary>
    Task<AnalysisResponse[]?> GetAnalysisAppVerSummary(
        DateOnly startTime,
        DateOnly endTime,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取活跃用户语言
    /// </summary>
    Task<AnalysisResponse[]?> GetAnalysisLocaleSummary(
        DateOnly startTime,
        DateOnly endTime,
        bool isall,
        string? appVersion,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取活跃用户设备
    /// </summary>
    Task<AnalysisResponse[]?> GetAnalysisEquipmentSummary(
        DateOnly startTime,
        DateOnly endTime,
        string? appVersion,
        bool isMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取活跃版本统计数据
    /// </summary>
    Task<AnalysisResponse[]?> GetAppVerStatisticsData(
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default);
}

partial interface IStatisticsRepository
{
    /// <summary>
    /// 获取系统中第一条活跃用户
    /// </summary>
    Task<ActiveUserAnonymousStatistic?> GetFirstRecord(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 每日统计
    /// </summary>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> StatisticsDaysData(DateOnly start, DateOnly end,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除指定区间的数据返回行数
    /// </summary>
    Task<int> DeleteDaysData(DateTimeOffset start, DateTimeOffset end,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 统计活跃用户版本数据
    /// </summary>
    Task<int> StatisticsAppVerDaysData(DateOnly start, DateOnly end,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取最后一条广告统计每日记录
    /// </summary>
    Task<KomaasharuStatisticPerDaySummary?> GetLastAdvertisementStatisticDaily(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取第一条广告统计记录
    /// </summary>
    Task<KomaasharuStatistic?> GetFirstAdvertisementStatistic(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 广告信息每日统计
    /// </summary>
    /// <param name="statisticDate">统计日期</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> PerformAdvertisementStatisticDaily(DateOnly statisticDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取最后一条活跃度统计记录
    /// </summary>
    Task<ActiveUserDayWeekMonthSummary?> GetLastUserDWMStatisticDaily(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 用户活跃度信息每日统计
    /// </summary>
    /// <param name="statisticDate">统计日期</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> PerformUserDWMStatisticDaily(DateOnly statisticDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取最后一条订单金额数量统计记录
    /// </summary>
    Task<OrderAmountQtySummary?> GetLastOrderAmountQtySummary(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 订单金额和数量每日统计
    /// </summary>
    /// <param name="statisticDate">统计日期</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> PerformOrderAmountQtyStatisticDaily(DateOnly statisticDate,
        CancellationToken cancellationToken = default);
}