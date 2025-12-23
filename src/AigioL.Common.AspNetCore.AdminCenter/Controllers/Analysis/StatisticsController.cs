using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Analysis;

/// <summary>
/// 大数据统计管理
/// </summary>
public static partial class StatisticsController
{
    const string ControllerName = ControllerConstants.StatisticsConsole;

    public static void MapKeyValuePair(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/basics/statistics")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("大数据统计管理");
    }

    ///// <summary>
    ///// 用户注册比例统计（用户所绑定的平台比例）
    ///// </summary>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerName + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<CachedStatistics<StatisticsPieResponse[]>>> UserStatistics([FromQuery] bool refresh = false)
    //{
    //    const string Key = $"{nameof(StatisticsController)}_UserStatistics";

    //    if (refresh)
    //    {
    //        memoryCache.Remove(Key);
    //    }
    //    var r = await memoryCache.GetOrCreateAsync(Key, async entry => CachedStatistics.Create(await statisticsRepo.GetUserStatisticsAsync()));

    //    return r;
    //}

    ///// <summary>
    ///// 注册用户统计
    ///// </summary>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerName + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<IEnumerable<StatisticsLineResponse>>> RegisterUserStatistics(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime)
    //{
    //    endTime = endTime.AddDays(1);
    //    if (startTime.AddMonths(1) > endTime)
    //        return "筛选天数不能大于1个月。";

    //    var r = await statisticsRepo.GetRegisterUserStatisticsAsync(startTime, endTime);

    //    return r;
    //}

    ///// <summary>
    ///// 活跃用户统计
    ///// </summary>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerName + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<IEnumerable<ActiveUserSumResponse>>> ActiveUserCountStatistics(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime)
    //{
    //    endTime = endTime.AddDays(1);
    //    var r = statisticsRepo.GetActiveUserStatisticsAsync(startTime, endTime);
    //    return await r;
    //}

    ///// <summary>
    ///// 获取活跃用户
    ///// </summary>
    ///// <param name="platform">平台</param>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    ///// <returns></returns>
    //[HttpGet("{platform?}"), PermissionFilter(ControllerName + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<StatisticsActiveUserOSResponse[]>> PlatformUserStatistics(
    //    [FromRoute] SppWebApiCompatDevicePlatform? platform,
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime)
    //{
    //    endTime = endTime.AddDays(1);
    //    var r = await statisticsRepo.GetActiveStatisticsAsync(platform, startTime, endTime);
    //    return r;
    //}

    ///// <summary>
    ///// 获取某天的用户系统分布
    ///// </summary>
    ///// <param name="platform">平台</param>
    ///// <param name="time">时间</param>
    ///// <returns></returns>
    //[HttpGet("{platform?}"), PermissionFilter(ControllerName + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<StatisticsChartActiveUserOSResponse[]>> OsVersionStatistics(
    //    [FromRoute] SppWebApiCompatDevicePlatform? platform,
    //    [FromQuery] DateTimeOffset time)
    //{
    //    var r = await statisticsRepo.GetOsVersionStatisticsAsync(platform, time);
    //    return r;
    //}

    ///// <summary>
    ///// 获取广告查看和点击统计
    ///// </summary>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerName + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<StatisticsAdvertisementResponse[]>> AdvertisementStatistics(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime)
    //{
    //    endTime = endTime.AddDays(1);
    //    var r = await statisticsRepo.GetAdvertisementStatisticsAsync(startTime, endTime);
    //    return r;
    //}

    ///// <summary>
    ///// 获取用户活跃度统计
    ///// </summary>
    ///// <param name="platform">平台</param>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    ///// <returns></returns>
    //[HttpGet("{platform?}"), PermissionFilter(ControllerName + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<UserActivityStatisticsResponse[]>> UserActivityStatistics(
    //    [FromRoute] SppWebApiCompatDevicePlatform? platform,
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime)
    //{
    //    endTime = endTime.AddDays(1);
    //    var r = await statisticsRepo.GetUserActivityStatisticsAsync(platform, startTime, endTime);
    //    return r;
    //}

    ///// <summary>
    ///// 获取短信使用趋势统计
    ///// </summary>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerName + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<StatisticsSmsUsageTrendResponse[]>> SmsUsageTrendStatistics(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime)
    //{
    //    endTime = endTime.AddDays(1);
    //    var r = await statisticsRepo.GetSmsUsageTrendStatisticsAsync(startTime, endTime);
    //    return r;
    //}

    ///// <summary>
    ///// 获取邮件发送趋势统计
    ///// </summary>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerName + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<StatisticsEmailUsageTrendResponse[]>> EmailUsageTrendStatistics(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime)
    //{
    //    endTime = endTime.AddDays(1);
    //    var r = await statisticsRepo.GetEmailUsageTrendStatisticsAsync(startTime, endTime);
    //    return r;
    //}

    ///// <summary>
    ///// 获取订单的金额数量统计
    ///// </summary>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    ///// <param name="unit">统计单位</param>
    ///// <param name="paymentType">支付类型</param>
    ///// <param name="taxed">税后</param>
    ///// <param name="pureProfit">纯利润</param>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerNameOrderSummary + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<object>> OrderAmountQtyStatistics(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime,
    //    [FromQuery] string unit = "day",
    //    [FromQuery] PaymentType? paymentType = null,
    //    [FromQuery] bool taxed = false,
    //    [FromQuery] bool pureProfit = false)
    //{
    //    endTime = endTime.AddDays(1);
    //    var r = await statisticsRepo.GetOrderAmountQtyStatisticsAsync(startTime, endTime, unit, paymentType, taxed, pureProfit);
    //    return r;
    //}

    ///// <summary>
    ///// 获取订单的金额数量统计表格
    ///// </summary>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    ///// <param name="orderType">订单类型</param>
    ///// <param name="orderBusinessType">订单类型类型</param>
    ///// <param name="orderBy"></param>
    ///// <param name="desc"></param>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerNameOrderSummary + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<object>> OrderAmountQtySummary(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime,
    //    [FromQuery] OrderType[]? orderType = null,
    //    [FromQuery] OrderBusinessType[]? orderBusinessType = null,
    //    [FromQuery] string? orderBy = null,
    //    [FromQuery] bool? desc = null)
    //{
    //    endTime = endTime.AddDays(1);
    //    var r = await statisticsRepo.GetOrderSummaryTable(startTime, endTime, orderType, orderBusinessType, orderBy, desc);
    //    return r;
    //}

    ///// <summary>
    ///// 获取活跃用户数据
    ///// </summary>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    ///// <param name="appVersion"></param>
    ///// <param name="appId"></param>
    ///// <param name="isMonth"></param>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerNameAnalysisLog + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<AnalysisResponse[]?>> GetStartServiceLogAnalysis(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime,
    //    [FromQuery] string? appVersion,
    //    [FromQuery] Guid? appId,
    //    [FromQuery] bool isMonth = false)
    //{
    //    var r = await statisticsRepo.GetStartServiceLogAnalysis(startTime, endTime, appVersion, appId, isMonth);
    //    return r;
    //}

    ///// <summary>
    ///// 获取开始会话统计数据
    ///// </summary>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    ///// <param name="appVersion"></param>
    ///// <param name="appId"></param>
    ///// <param name="isMonth">获取月度统计</param>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerNameAnalysisLog + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<AnalysisResponse[]?>> GetStartSessionLogAnalysis(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime,
    //    [FromQuery] string? appVersion,
    //    [FromQuery] Guid? appId,
    //    [FromQuery] bool isMonth = false)
    //{
    //    var r = await statisticsRepo.GetStartSessionLogAnalysis(startTime, endTime, appVersion, appId, isMonth);
    //    return r;
    //}

    ///// <summary>
    ///// 获取事件分析一级菜单
    ///// </summary>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    ///// <param name="appVersion"></param>
    ///// <param name="appId"></param>
    ///// <param name="isMonth"></param>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerNameAnalysisEvent + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<List<string>?>> GetAnalysisEvnetMenuList(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime,
    //    [FromQuery] string? appVersion,
    //    [FromQuery] Guid? appId,
    //    [FromQuery] bool isMonth = false)
    //{
    //    var r = await statisticsRepo.GetAnalysisEvnetMenuList(startTime, endTime, appVersion, appId, isMonth);
    //    return r;
    //}

    ///// <summary>
    ///// 获取事件属性菜单
    ///// </summary>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    ///// <param name="appVersion">版本号</param>
    ///// <param name="eventNames">事件名称</param>
    ///// <param name="appId"></param>
    ///// <param name="isMonth"></param>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerNameAnalysisEvent + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<List<string>?>> GetAnalysisPropertiesKeyMenuList(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime,
    //    [FromQuery] string? appVersion,
    //    [FromQuery] string? eventNames,
    //    [FromQuery] Guid? appId,
    //    [FromQuery] bool isMonth = false)
    //{
    //    var r = await statisticsRepo.GetAnalysisPropertiesKeyMenuList(startTime, endTime, appVersion, eventNames, appId, isMonth);
    //    return r;
    //}

    ///// <summary>
    ///// 获取事件属性 Value 菜单
    ///// </summary>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    ///// <param name="appVersion">版本号</param>
    ///// <param name="eventNames">事件名称</param>
    ///// <param name="key"></param>
    ///// <param name="appId"></param>
    ///// <param name="isMonth"></param>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerNameAnalysisEvent + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<List<string>?>> GetAnalysisPropertiesValueMenuList(
    //    [FromQuery] DateTimeOffset? startTime,
    //    [FromQuery] DateTimeOffset? endTime,
    //    [FromQuery] string? appVersion,
    //    [FromQuery] string? eventNames,
    //    [FromQuery] string? key,
    //    [FromQuery] Guid? appId,
    //    [FromQuery] bool isMonth = false)
    //{
    //    var r = await statisticsRepo.GetAnalysisPropertiesValueMenuList(startTime, endTime, appVersion, eventNames, key, appId, isMonth);
    //    return r;
    //}

    ///// <summary>
    ///// 获取事件统计数据
    ///// </summary>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    ///// <param name="eventNames"></param>
    ///// <param name="appVersion"></param>
    ///// <param name="key"></param>
    ///// <param name="value"></param>
    ///// <param name="appId"></param>
    ///// <param name="isMonth"></param>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerNameAnalysisEvent + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<AnalysisResponse[]?>> GetAnalysisEventSummary(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime,
    //    [FromQuery] string? eventNames,
    //    [FromQuery] string? appVersion,
    //    [FromQuery] string? key,
    //    [FromQuery] string? value,
    //    [FromQuery] Guid? appId,
    //    [FromQuery] bool isMonth = false)
    //{
    //    var r = await statisticsRepo.GetAnalysisEventSummary(startTime, endTime, appVersion, eventNames, key, value, appId, isMonth);
    //    return r;
    //}

    ///// <summary>
    ///// 获取平台数据
    ///// </summary>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerNameAnalysisLog + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<OptionItem[]?>> GetAppIdList()
    //{
    //    var r = await analysisLogRropo.GetAppIdList();
    //    return r;
    //}

    ///// <summary>
    ///// 获取版本
    ///// </summary>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerNameAnalysisLog + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<List<string>?>> GetAnalysisAppVer(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime)
    //{
    //    var r = await statisticsRepo.GetAnalysisAppVer(startTime, endTime);
    //    return r;
    //}

    ///// <summary>
    ///// 获取版本使用趋势
    ///// </summary>
    //[HttpGet, PermissionFilter(ControllerNameAnalysisLog + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<AnalysisResponse[]?>> GetAnalysisAppVerSummary(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime,
    //    [FromQuery] Guid? appId,
    //    [FromQuery] bool isMonth = false)
    //{
    //    var r = await statisticsRepo.GetAnalysisAppVerSummary(startTime, endTime, appId, isMonth);
    //    return r;
    //}

    ///// <summary>
    ///// 获取活跃用户语言
    ///// </summary>
    //[HttpGet, PermissionFilter(ControllerNameAnalysisLog + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<AnalysisResponse[]?>> GetAnalysisLocaleSummary(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime,
    //    [FromQuery] bool isall,
    //    [FromQuery] string? appVersion,
    //    [FromQuery] Guid? appId,
    //    [FromQuery] bool isMonth = false)
    //{
    //    var r = await statisticsRepo.GetAnalysisLocaleSummary(startTime, endTime, isall, appVersion, appId, isMonth);
    //    return r;
    //}

    ///// <summary>
    ///// 获取活跃用户设备
    ///// </summary>
    //[HttpGet, PermissionFilter(ControllerNameAnalysisLog + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<AnalysisResponse[]?>> GetAnalysisEquipmentSummary(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime,
    //    [FromQuery] string? appVersion,
    //    [FromQuery] bool isMonth = false)
    //{
    //    var r = await statisticsRepo.GetAnalysisEquipmentSummary(startTime, endTime, appVersion, isMonth);
    //    return r;
    //}

    ///// <summary>
    ///// 统计活跃版本数据
    ///// </summary>
    ///// <param name="startTime">开始时间</param>
    ///// <param name="endTime">结束时间</param>
    ///// <returns></returns>
    //[HttpGet, PermissionFilter(ControllerName + nameof(BMButtonType.Query))]
    //public async Task<BMApiRsp<AnalysisResponse[]?>> GetAppVerStatisticsData(
    //    [FromQuery] DateTimeOffset startTime,
    //    [FromQuery] DateTimeOffset endTime)
    //{
    //    var r = await statisticsRepo.GetAppVerStatisticsData(startTime, endTime);
    //    return r;
    //}
}


file sealed partial record CachedStatistics<T>(T Data, DateTimeOffset CacheTime);

file sealed partial record CachedStatistics
{
    public static CachedStatistics<T> Create<T>(T t) => new(t, DateTimeOffset.Now);
}