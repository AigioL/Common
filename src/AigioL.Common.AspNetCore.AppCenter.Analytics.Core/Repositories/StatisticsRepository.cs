using AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.Statistics;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities.Komaasharus.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using static AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.Abstractions.IStatisticsRepository;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories;

sealed partial class StatisticsRepository<TDbContext>(TDbContext dbContext, IServiceProvider serviceProvider) :
    Repository<TDbContext, ActiveUserStatisticSummary, Guid>(dbContext, serviceProvider),
    IStatisticsRepository
    where TDbContext : DbContext,
    IActiveUsersDbContext,
    IActiveUsersSummariesDbContext,
    IKomaasharuSummariesDbContext,
    IIdentityDbContext
{
    /// <summary>
    /// <see cref="ExternalLoginChannel"/> 到 <see cref="StatisticUserCount"/> 的字典
    /// </summary>
    static readonly Dictionary<ExternalLoginChannel, StatisticUserCount> dictFLCToSUC = new()
    {
        { ExternalLoginChannel.Steam, StatisticUserCount.BindSteam },
        { ExternalLoginChannel.QQ, StatisticUserCount.BindQQ },
        { ExternalLoginChannel.Microsoft, StatisticUserCount.BindMS },
        { ExternalLoginChannel.Alipay, StatisticUserCount.BindAlipay },
        { ExternalLoginChannel.Weixin, StatisticUserCount.BindWeChat },
    };
}

partial class StatisticsRepository<TDbContext>
{
    public async Task<StatisticsPieResponse[]> GetUserStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        // 统计总用户、绑定手机号码的、绑定邮件的
        var queryUsers = from m in db.Users.AsNoTrackingWithIdentityResolution()
                         where m.DeleteTime == null
                         select m;

        // 统计外部账号绑定的
        var queryExtAccs = from m in db.ExternalAccounts.AsNoTrackingWithIdentityResolution()
                           where m.UserId != null // 已绑定用户（非注销用户）
                           select m;

        StatisticUserCount[] userCountEnums =
        [
            StatisticUserCount.UserCount,
            StatisticUserCount.BindEmail,
            StatisticUserCount.BindPhone,
        ];

        var userCountEnumHashSet = userCountEnums.Concat(dictFLCToSUC.Values).ToHashSet();
        var statistics = new StatisticsPieResponse[userCountEnumHashSet.Count];

        async Task<int> CalcCountAsync(StatisticUserCount statisticUserCount)
        {
            foreach (var it in dictFLCToSUC)
            {
                if (it.Value == statisticUserCount)
                {
                    var count = await queryExtAccs.CountAsync(
                        x => x.Type == it.Key,
                        cancellationToken);
                    return count;
                }
            }

            return 0;
        }

        for (int i = 0; i < userCountEnumHashSet.Count; i++)
        {
            var it = userCountEnums[i];
            var count = it switch
            {
                StatisticUserCount.UserCount => await queryUsers.CountAsync(cancellationToken),
                StatisticUserCount.BindPhone => await queryUsers.CountAsync(
                                        u => u.PhoneNumberConfirmed || u.PhoneNumber != null,
                                        cancellationToken),
                StatisticUserCount.BindEmail => await queryUsers.CountAsync(
                                        u => u.EmailConfirmed || u.Email != null,
                                        cancellationToken),
                _ => await CalcCountAsync(it),
            };
            statistics[i] = new()
            {
                Name = it.GetDescription() ?? it.ToString(),
                Count = count,
            };
        }

        return statistics;
    }

    public async Task<StatisticsLineResponse[]> GetRegisterUserStatisticsAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default)
    {
        var queryUsers = from m in db.Users.AsNoTrackingWithIdentityResolution()
                         where m.CreateTime >= startTime && m.CreateTime < endTime
                         let date = m.CreateTime.ToOffset(TimeSpan.FromHours(8)).Date
                         group m by date into g
                         select new StatisticsLineResponse
                         {
                             Time = g.Key,
                             Count = g.LongCount(),
                         };
#if DEBUG
        var sqlString = queryUsers.ToQueryString();
#endif
        var r = await queryUsers.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<ActiveUserSumResponse[]> GetActiveUserStatisticsAsync(
        DateOnly startTime,
        DateOnly endTime,
        CancellationToken cancellationToken = default)
    {
        var query = db.ActiveUserStatisticSummaries
            .AsNoTrackingWithIdentityResolution()
           .Where(x => x.StatisticsStartTime >= startTime && x.StatisticsStartTime < endTime)
           .OrderBy(x => x.StatisticsStartTime)
           .Select(ActiveUserStatisticSummary.Expression);
#if DEBUG
        var sqlString = query.ToQueryString();
#endif
        var r = await query.ToArrayAsync(RequestAborted);
        return r;
    }

    public async Task<StatisticsActiveUserOSResponse[]> GetActiveStatisticsAsync(
#pragma warning disable CS0618 // 类型或成员已过时
        WebApiCompatDevicePlatform? platform,
#pragma warning restore CS0618 // 类型或成员已过时
        DateOnly startTime,
        DateOnly endTime,
        CancellationToken cancellationToken = default)
    {
        var query = db.ActiveUserPlatformSummaries
           .AsNoTrackingWithIdentityResolution()
           .Where(x => x.StatisticsTime >= startTime && x.StatisticsTime < endTime);
        if (platform.HasValue)
            query = query.Where(x => x.Platform == platform);

        var query2 = query
            .GroupBy(a => new
            {
                a.DeviceIdiom,
                a.StatisticsTime,
            })
            .Select(g => new StatisticsActiveUserOSResponse
            {
                DeviceIdiom = g.Key.DeviceIdiom,
                StatisticsTime = g.Key.StatisticsTime,
                Count = g.Sum(a => a.Count),
            })
            .OrderBy(x => x.StatisticsTime)
            .ThenBy(x => x.DeviceIdiom);
#if DEBUG
        var sqlString = query2.ToQueryString();
#endif
        var r = await query2.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<StatisticsChartActiveUserOSResponse[]> GetOsVersionStatisticsAsync(
#pragma warning disable CS0618 // 类型或成员已过时
        WebApiCompatDevicePlatform? platform,
#pragma warning restore CS0618 // 类型或成员已过时
        DateOnly time,
        CancellationToken cancellationToken = default)
    {
        var query = db.ActiveUserOSSummaries
          .AsNoTrackingWithIdentityResolution();

        if (platform.HasValue)
            query = query.Where(x => x.Platform == platform);

        var query2 = query
            .Where(x => x.StatisticsTime == time)
            .GroupBy(x => new { x.Platform, x.OSVersionSection1/*, x.OSVersionSection2*/ })
            .OrderBy(x => x.Key.Platform)
            .ThenBy(x => x.Key.OSVersionSection1)
            .Select(x => new StatisticsChartActiveUserOSResponse
            {
                Count = x.Sum(m => m.Count),
                Platform = x.Key.Platform,
                Version = x.Key.OSVersionSection1// + "." + x.Key.OSVersionSection2,
            });
#if DEBUG
        var sqlString = query2.ToQueryString();
#endif
        var r = await query2.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<StatisticsKomaasharuResponse[]> GetAdvertisementStatisticsAsync(
        DateOnly startTime,
        DateOnly endTime,
        CancellationToken cancellationToken = default)
    {
        var query = db.KomaasharuStatisticPerDaySummaries
            .AsNoTrackingWithIdentityResolution()
            .Where(x => startTime <= x.StatisticsTime && x.StatisticsTime < endTime);

        var query2 = query
            .Select(KomaasharuStatisticPerDaySummary.Expression)
            .GroupBy(a => a.StatisticsTime)
            .Select(g => new StatisticsKomaasharuResponse
            {
                StatisticsTime = g.Key,
                ClickCount = g.Sum(a => a.ClickCount),
                ViewCount = g.Sum(a => a.ViewCount)
            })
            .OrderBy(x => x.StatisticsTime);
#if DEBUG
        var sqlString = query2.ToQueryString();
#endif
        var r = await query2.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<UserActivityStatisticsResponse[]> GetUserActivityStatisticsAsync(
#pragma warning disable CS0618 // 类型或成员已过时
        WebApiCompatDevicePlatform? platform,
#pragma warning restore CS0618 // 类型或成员已过时
        DateOnly startTime,
        DateOnly endTime,
        CancellationToken cancellationToken = default)
    {
        var query = db.ActiveUserDayWeekMonthSummaries
             .AsNoTrackingWithIdentityResolution()
             .Where(x => x.StatisticsTime >= startTime && x.StatisticsTime < endTime);
        if (platform.HasValue)
            query = query.Where(x => x.Platform == platform);
        var query2 = query
            .Where(a => a.AUType != ActiveUserStatisticsType.AU_Month)
            .GroupBy(a => new { a.StatisticsTime, a.AUType })
            .Select(g => new ActiveUserDayWeekMonthSummary
            {
                StatisticsTime = g.Key.StatisticsTime,
                AUType = g.Key.AUType,
                ActiveUserCount = g.Sum(a => a.ActiveUserCount),
            })
            .Select(ActiveUserDayWeekMonthSummary.Expression)
            .OrderBy(a => a.StatisticsTime)
            .ThenBy(a => a.AUType);
#if DEBUG
        var sqlString = query2.ToQueryString();
#endif
        var r = await query2.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<StatisticsSmsUsageTrendResponse[]> GetSmsUsageTrendStatisticsAsync(
        DateOnly startTime,
        DateOnly endTime,
        CancellationToken cancellationToken = default)
    {
        var startTime_ = ToUTC8Date(startTime);
        var endTime_ = ToUTC8Date(endTime);

        var query = db.AuthMessageRecords.Where(r => r.Type == AuthMessageType.PhoneNumber)
            .AsNoTrackingWithIdentityResolution();

        var query2 = query
            .Where(x => x.Type == AuthMessageType.PhoneNumber)
            .Where(x => startTime_ <= x.CreateTime && x.CreateTime < endTime_)
            .GroupBy(x => new { x.CreateTime.ToOffset(TimeSpan.FromHours(8)).Date, x.RequestType })
            .Select(g => new StatisticsSmsUsageTrendResponse
            {
                RequestType = g.Key.RequestType,
                StatisticsTime = g.Key.Date,
                Count = g.Count(),
            })
            .OrderBy(x => x.StatisticsTime)
            .ThenBy(x => x.RequestType);
#if DEBUG
        var sqlString = query2.ToQueryString();
#endif
        var r = await query2.ToArrayAsync(cancellationToken);
        return r;
    }


    public async Task<StatisticsEmailUsageTrendResponse[]> GetEmailUsageTrendStatisticsAsync(
        DateOnly startTime,
        DateOnly endTime,
        CancellationToken cancellationToken = default)
    {
        //        var query = db.EmailSendRecords.AsNoTrackingWithIdentityResolution();

        //        var query2 = query
        //            .Where(x => x.SendStatus == EmailStatus.Sent)
        //            .Where(x => startTime <= x.SendTime && x.SendTime < endTime)
        //            .GroupBy(x => new { x.SendTime!.Value.Date, x.TemplateId, TemplateName = x.Template!.Name })
        //            .Select(g => new StatisticsEmailUsageTrendResponse
        //            {
        //                TemplateId = g.Key.TemplateId,
        //                TemplateName = g.Key.TemplateName,
        //                StatisticsTime = g.Key.Date,
        //                Count = g.Count(),
        //            })
        //            .OrderBy(x => x.StatisticsTime)
        //            .ThenBy(x => x.TemplateId)
        //#if DEBUG
        //        var sqlString = query2.ToQueryString();
        //#endif
        //        var r = await query2.ToArrayAsync(cancellationToken);
        //        return r;

        throw new NotImplementedException();
    }

    public async Task<StatisticsOrderAmountQtyModel[]> GetOrderAmountQtyStatisticsAsync(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        string? unit,
        PaymentType? paymentType,
        bool taxed,
        bool pureProfit,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<OrderAmountQtyTableModel[]> GetOrderSummaryTable(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        OrderType[]? orderTypes = null,
        int[]? orderBusinessTypeIds = null,
        string? orderBy = null,
        bool? desc = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<AnalysisResponse[]?> GetStartServiceLogAnalysis(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        string? appVersion,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<AnalysisResponse[]?> GetStartSessionLogAnalysis(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        string? appVersion,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<string[]?> GetAnalysisEvnetMenuList(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        string? appVersion,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<string[]?> GetAnalysisPropertiesKeyMenuList(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        string? appVersion,
        string? eventNames,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<string[]?> GetAnalysisPropertiesValueMenuList(
         DateTimeOffset? startTime,
         DateTimeOffset? endTime,
         string? appVersion,
         string? eventNames,
         string? key,
         Guid? appId,
         bool isMonth,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<AnalysisResponse[]?> GetAnalysisEventSummary(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        string? appVersion,
        string? eventNames,
        string? key,
        string? value,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<string[]?> GetAnalysisAppVer(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<AnalysisResponse[]?> GetAnalysisAppVerSummary(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<AnalysisResponse[]?> GetAnalysisLocaleSummary(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        bool isall,
        string? appVersion,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<AnalysisResponse[]?> GetAnalysisEquipmentSummary(
        DateTimeOffset? startTime,
        DateTimeOffset? endTime,
        string? appVersion,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<AnalysisResponse[]?> GetAppVerStatisticsData(
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

partial class StatisticsRepository<TDbContext>
{
    public async Task<ActiveUserAnonymousStatistic?> GetFirstRecord(
        CancellationToken cancellationToken = default)
    {
        var query = db.ActiveUserRecords
            .AsNoTrackingWithIdentityResolution()
            .OrderBy(x => x.CreateTime)
            .Take(1);
#if DEBUG
        var sqlString = query.ToQueryString();
#endif
        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<int> StatisticsDaysData(DateTimeOffset start, DateTimeOffset end)
    {
        throw new NotImplementedException();
    }

    public async Task<int> DeleteDaysData(DateTimeOffset start, DateTimeOffset end)
    {
        throw new NotImplementedException();
    }

    public async Task<int> StatisticsAppVerDaysData(DateTimeOffset start, DateTimeOffset end)
    {
        throw new NotImplementedException();
    }

    public async Task<KomaasharuStatisticPerDaySummary?> GetLastAdvertisementStatisticDaily(
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<KomaasharuStatistic?> GetFirstAdvertisementStatistic(
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<int> PerformAdvertisementStatisticDaily(DateTimeOffset statisticDate)
    {
        throw new NotImplementedException();
    }

    public async Task<ActiveUserDayWeekMonthSummary?> GetLastUserDWMStatisticDaily(
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<int> PerformUserDWMStatisticDaily(DateTimeOffset statisticDate)
    {
        throw new NotImplementedException();
    }

    public async Task<OrderAmountQtySummary?> GetLastOrderAmountQtySummary(
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<int> PerformOrderAmountQtyStatisticDaily(DateTimeOffset statisticDate)
    {
        throw new NotImplementedException();
    }
}