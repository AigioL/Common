using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.Statistics;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Analysis;

/// <summary>
/// 大数据统计管理
/// </summary>
public static partial class StatisticsController
{
    const string ControllerName = ControllerConstants.StatisticsConsole;
    const string ControllerNameAnalysisEvent = ControllerConstants.AnalysisEvent;
    const string ControllerNameAnalysisLog = ControllerConstants.AnalysisLog;
    const string ControllerNameOrderSummary = ControllerConstants.OrderSummary;

    public static void MapAnalysisStatistics(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/basics/statistics")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("大数据统计管理");

        routeGroup.MapGet("userstatistics", async (HttpContext context,
            [FromQuery] bool refresh = false) =>
        {
            var memoryCache = context.RequestServices.GetRequiredService<IMemoryCache>();
            const string Key = $"{nameof(StatisticsController)}_UserStatistics";
            if (refresh)
            {
                memoryCache.Remove(Key);
            }
            BMApiRsp<CachedStatistics<StatisticsPieResponse[]>?> r = await memoryCache.GetOrCreateAsync(Key, async entry =>
            {
                var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
                var r = await statisticsRepo.GetUserStatisticsAsync(context.RequestAborted);
                return CachedStatistics.Create(r);
            });
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("客户端用户注册比例统计（绑定的第三方平台账号比例）");

        routeGroup.MapGet("registeruserstatistics", async (HttpContext context,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime) =>
        {
            endTime = endTime.AddDays(1);
            if (startTime.AddMonths(1) > endTime)
            {
                return "筛选天数不能大于 1 个月";
            }
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            BMApiRsp<StatisticsLineResponse[]?> r = await statisticsRepo.GetRegisterUserStatisticsAsync(startTime, endTime, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("客户端注册用户统计");

        routeGroup.MapGet("activeusercountstatistics", async (HttpContext context,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            BMApiRsp<ActiveUserSumResponse[]?> r = await statisticsRepo.GetActiveUserStatisticsAsync(startTime, endTime, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("客户端活跃用户汇总统计");

        routeGroup.MapGet("platformuserstatistics/{platform?}", async (HttpContext context,
#pragma warning disable CS0618 // 类型或成员已过时
            [FromRoute] WebApiCompatDevicePlatform? platform,
#pragma warning restore CS0618 // 类型或成员已过时
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            BMApiRsp<StatisticsActiveUserOSResponse[]?> r = await statisticsRepo.GetActiveStatisticsAsync(platform, startTime, endTime, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("客户端用户使用的操作系统平台统计");

        routeGroup.MapGet("osversionstatistics/{platform?}", async (HttpContext context,
#pragma warning disable CS0618 // 类型或成员已过时
            [FromRoute] WebApiCompatDevicePlatform? platform,
#pragma warning restore CS0618 // 类型或成员已过时
            [FromQuery] DateTimeOffset time) =>
        {
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            BMApiRsp<StatisticsChartActiveUserOSResponse[]?> r = await statisticsRepo.GetOsVersionStatisticsAsync(platform, time, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("查询某一天的客户端用户使用的操作系统平台分布");

        routeGroup.MapGet("advertisementstatistics", async (HttpContext context,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            BMApiRsp<StatisticsKomaasharuResponse[]?> r = await statisticsRepo.GetAdvertisementStatisticsAsync(startTime, endTime, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("查询广告查看和点击统计");

        routeGroup.MapGet("useractivitystatistics/{platform?}", async (HttpContext context,
#pragma warning disable CS0618 // 类型或成员已过时
            [FromRoute] WebApiCompatDevicePlatform? platform,
#pragma warning restore CS0618 // 类型或成员已过时
             [FromQuery] DateTimeOffset startTime,
             [FromQuery] DateTimeOffset endTime) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            BMApiRsp<UserActivityStatisticsResponse[]?> r = await statisticsRepo.GetUserActivityStatisticsAsync(platform, startTime, endTime, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("客户端活跃度用户统计");

        routeGroup.MapGet("smsusagetrendstatistics", async (HttpContext context,
             [FromQuery] DateTimeOffset startTime,
             [FromQuery] DateTimeOffset endTime) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            BMApiRsp<StatisticsSmsUsageTrendResponse[]?> r = await statisticsRepo.GetSmsUsageTrendStatisticsAsync(startTime, endTime, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("短信验证码发送趋势统计");

        routeGroup.MapGet("emailusagetrendstatistics", async (HttpContext context,
             [FromQuery] DateTimeOffset startTime,
             [FromQuery] DateTimeOffset endTime) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            BMApiRsp<StatisticsEmailUsageTrendResponse[]?> r = await statisticsRepo.GetEmailUsageTrendStatisticsAsync(startTime, endTime, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("邮件验证码发送趋势统计");

        routeGroup.MapGet("orderamountqtystatistics", async (HttpContext context,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime,
            [FromQuery] string unit = "day",
            [FromQuery] PaymentType? paymentType = null,
            [FromQuery] bool taxed = false,
            [FromQuery] bool pureProfit = false) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            BMApiRsp<StatisticsOrderAmountQtyModel[]?> r = await statisticsRepo.GetOrderAmountQtyStatisticsAsync(startTime, endTime, unit, paymentType, taxed, pureProfit, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerNameOrderSummary, BMButtonType.Query)
        .WithDescription("订单的金额数量统计");

        routeGroup.MapGet("orderamountqtysummary", async (HttpContext context,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime,
            [FromQuery] OrderType[]? orderType = null,
            [FromQuery] int[]? orderBusinessType = null,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            BMApiRsp<OrderAmountQtyTableModel[]?> r = await statisticsRepo.GetOrderSummaryTable(startTime, endTime, orderType, orderBusinessType, orderBy, desc, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerNameOrderSummary, BMButtonType.Query)
        .WithDescription("订单的金额数量统计表格项");

        routeGroup.MapGet("getstartserviceloganalysis", async (HttpContext context,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime,
            [FromQuery] string? appVersion,
            [FromQuery] Guid? appId,
            [FromQuery] bool isMonth = false) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            BMApiRsp<AnalysisResponse[]?> r = await statisticsRepo.GetStartServiceLogAnalysis(startTime, endTime, appVersion, appId, isMonth, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerNameAnalysisLog, BMButtonType.Query)
        .WithDescription("活跃用户数据");

        routeGroup.MapGet("getstartsessionloganalysis", async (HttpContext context,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime,
            [FromQuery] string? appVersion,
            [FromQuery] Guid? appId,
            [FromQuery] bool isMonth = false) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            BMApiRsp<AnalysisResponse[]?> r = await statisticsRepo.GetStartSessionLogAnalysis(startTime, endTime, appVersion, appId, isMonth, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerNameAnalysisLog, BMButtonType.Query)
        .WithDescription("开始会话统计数据");

        routeGroup.MapGet("getanalysisevnetmenulist", async (HttpContext context,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime,
            [FromQuery] string? appVersion,
            [FromQuery] Guid? appId,
            [FromQuery] bool isMonth = false) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            var r = await statisticsRepo.GetAnalysisEvnetMenuList(startTime, endTime, appVersion, appId, isMonth, context.RequestAborted);
            return BMApiRsp.OK(r);
        }).PermissionFilter(ControllerNameAnalysisEvent, BMButtonType.Query)
        .WithDescription("事件分析一级菜单");

        routeGroup.MapGet("getanalysispropertieskeymenulist", async (HttpContext context,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime,
            [FromQuery] string? appVersion,
            [FromQuery] string? eventNames,
            [FromQuery] Guid? appId,
            [FromQuery] bool isMonth = false) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            var r = await statisticsRepo.GetAnalysisPropertiesKeyMenuList(startTime, endTime, appVersion, eventNames, appId, isMonth, context.RequestAborted);
            return BMApiRsp.OK(r);
        }).PermissionFilter(ControllerNameAnalysisEvent, BMButtonType.Query)
        .WithDescription("事件属性键菜单");

        routeGroup.MapGet("getanalysispropertiesvaluemenulist", async (HttpContext context,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime,
            [FromQuery] string? appVersion,
            [FromQuery] string? eventNames,
            [FromQuery] string? key,
            [FromQuery] Guid? appId,
            [FromQuery] bool isMonth = false) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            var r = await statisticsRepo.GetAnalysisPropertiesValueMenuList(startTime, endTime, appVersion, eventNames, key, appId, isMonth, context.RequestAborted);
            return BMApiRsp.OK(r);
        }).PermissionFilter(ControllerNameAnalysisEvent, BMButtonType.Query)
        .WithDescription("事件属性值菜单");

        routeGroup.MapGet("getanalysiseventsummary", async (HttpContext context,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime,
            [FromQuery] string? appVersion,
            [FromQuery] string? eventNames,
            [FromQuery] string? key,
            [FromQuery] string? value,
            [FromQuery] Guid? appId,
            [FromQuery] bool isMonth = false) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            var r = await statisticsRepo.GetAnalysisEventSummary(startTime, endTime, appVersion, eventNames, key, value, appId, isMonth, context.RequestAborted);
            return BMApiRsp.OK(r);
        }).PermissionFilter(ControllerNameAnalysisEvent, BMButtonType.Query)
        .WithDescription("事件统计数据");

        routeGroup.MapGet("getanalysisappver", async (HttpContext context,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            var r = await statisticsRepo.GetAnalysisAppVer(startTime, endTime, context.RequestAborted);
            return BMApiRsp.OK(r);
        }).PermissionFilter(ControllerNameAnalysisLog, BMButtonType.Query)
        .WithDescription("版本统计数据");

        routeGroup.MapGet("getanalysisappversummary", async (HttpContext context,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime,
            [FromQuery] Guid? appId,
            [FromQuery] bool isMonth = false) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            var r = await statisticsRepo.GetAnalysisAppVerSummary(startTime, endTime, appId, isMonth, context.RequestAborted);
            return BMApiRsp.OK(r);
        }).PermissionFilter(ControllerNameAnalysisLog, BMButtonType.Query)
        .WithDescription("版本使用趋势");

        routeGroup.MapGet("getanalysislocalesummary", async (HttpContext context,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime,
            [FromQuery] bool isall,
            [FromQuery] string? appVersion,
            [FromQuery] Guid? appId,
            [FromQuery] bool isMonth = false) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            var r = await statisticsRepo.GetAnalysisLocaleSummary(startTime, endTime, isall, appVersion, appId, isMonth, context.RequestAborted);
            return BMApiRsp.OK(r);
        }).PermissionFilter(ControllerNameAnalysisLog, BMButtonType.Query)
        .WithDescription("活跃用户语言");

        routeGroup.MapGet("getanalysisequipmentsummary", async (HttpContext context,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime,
            [FromQuery] string? appVersion,
            [FromQuery] bool isMonth = false) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            var r = await statisticsRepo.GetAnalysisEquipmentSummary(startTime, endTime, appVersion, isMonth, context.RequestAborted);
            return BMApiRsp.OK(r);
        }).PermissionFilter(ControllerNameAnalysisLog, BMButtonType.Query)
        .WithDescription("活跃用户设备");

        routeGroup.MapGet("getappverstatisticsdata", async (HttpContext context,
            [FromQuery] DateTimeOffset startTime,
            [FromQuery] DateTimeOffset endTime) =>
        {
            endTime = endTime.AddDays(1);
            var statisticsRepo = context.RequestServices.GetRequiredService<IStatisticsRepository>();
            var r = await statisticsRepo.GetAppVerStatisticsData(startTime, endTime, context.RequestAborted);
            return BMApiRsp.OK(r);
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("活跃用户设备");
    }

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
}