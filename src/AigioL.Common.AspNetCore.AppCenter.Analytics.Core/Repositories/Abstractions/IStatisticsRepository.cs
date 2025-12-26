using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.Statistics;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus.Summaries;
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

    /// <summary>
    /// 获取短信使用趋势统计
    /// </summary>
    Task<StatisticsSmsUsageTrendResponse[]> GetSmsUsageTrendStatisticsAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取邮件发送趋势统计
    /// </summary>
    Task<StatisticsEmailUsageTrendResponse[]> GetEmailUsageTrendStatisticsAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取订单的金额数量统计
    /// </summary>
    Task<StatisticsOrderAmountQtyModel[]> GetOrderAmountQtyStatisticsAsync(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        string? unit,
        PaymentType? paymentType,
        bool taxed,
        bool pureProfit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取订单的金额数量统计表格
    /// </summary>
    Task<OrderAmountQtyTableModel[]> GetOrderSummaryTable(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        OrderType[]? orderTypes = null,
        int[]? orderBusinessTypeIds = null,
        string? orderBy = null,
        bool? desc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取启动服务数据
    /// </summary>
    Task<AnalysisResponse[]?> GetStartServiceLogAnalysis(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        string? appVersion,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取开始会话数据
    /// </summary>
    Task<AnalysisResponse[]?> GetStartSessionLogAnalysis(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        string? appVersion,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取事件的菜单
    /// </summary>
    Task<string[]?> GetAnalysisEvnetMenuList(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        string? appVersion,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取事件属性菜单
    /// </summary>
    Task<string[]?> GetAnalysisPropertiesKeyMenuList(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        string? appVersion,
        string? eventNames,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取事件属性 Value 菜单
    /// </summary>
    Task<string[]?> GetAnalysisPropertiesValueMenuList(
         DateTimeOffset? startTime,
         DateTimeOffset? endTime,
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
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
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
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取版本使用趋势
    /// </summary>
    Task<AnalysisResponse[]?> GetAnalysisAppVerSummary(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取活跃用户语言
    /// </summary>
    Task<AnalysisResponse[]?> GetAnalysisLocaleSummary(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        bool isall,
        string? appVersion,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取活跃用户设备
    /// </summary>
    Task<AnalysisResponse[]?> GetAnalysisEquipmentSummary(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        string? appVersion,
        bool isMonth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取活跃版本统计数据
    /// </summary>
    Task<AnalysisResponse[]?> GetAppVerStatisticsData(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default);
}