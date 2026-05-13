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
using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.LazyCalendar;
using static AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.LazyStatisticUserCountDict;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories;

sealed partial class StatisticsRepository<TDbContext>(TDbContext dbContext, IServiceProvider serviceProvider) :
    Repository<TDbContext, ActiveUserStatisticSummary, Guid>(dbContext, serviceProvider),
    IStatisticsRepository
    where TDbContext : DbContext,
    IActiveUsersDbContext,
    IActiveUsersSummariesDbContext,
    IKomaasharuSummariesDbContext,
    IIdentityDbContext,
    IOrderSummariesDbContext,
    IKeyValuePairsDbContext,
    IOrderingPaymentBaseDbContext,
    IAnalysisLogSummariesDbContext
{
    /// <summary>
    /// 设置通用很长的超时时间，统计相关执行 SQL 可能非常耗时
    /// </summary>
    void SetCommandTimeout()
    {
        db.Database.SetCommandTimeout(TimeSpan.FromHours(12));
    }
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

        int i = 0;
        foreach (var it in userCountEnumHashSet)
        {
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
            i++;
        }

        return statistics;
    }

    public Task<StatisticsLineResponse[]> GetRegisterUserStatisticsAsync(
        DateOnly startTime,
        DateOnly endTime,
        CancellationToken cancellationToken = default)
    {
        var query = db.ActiveUserStatisticSummaries
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.StatisticsStartTime >= startTime && x.StatisticsStartTime <= endTime)
            .OrderBy(x => x.StatisticsStartTime)
            .Select(x => new StatisticsLineResponse
            {
                Time = x.StatisticsStartTime.ToUTC8Date(),
                Count = x.RegisterUserCount,
            });
#if DEBUG
        var sqlString = query.ToQueryString();
#endif
        return query.ToArrayAsync(cancellationToken);
    }

    public async Task<StatisticsLineResponse[]> GetRegisterUserStatisticsAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default)
    {
        var startDate = DateOnly.FromDateTime(startTime.Date);
        var endDateExclusive = DateOnly.FromDateTime(endTime.Date);
        if (endDateExclusive <= startDate)
        {
            return [];
        }

        return await GetRegisterUserStatisticsAsync(startDate, endDateExclusive.AddDays(-1), cancellationToken);
    }

    public async Task<ActiveUserSumResponse[]> GetActiveUserStatisticsAsync(
        DateOnly startTime,
        DateOnly endTime,
        CancellationToken cancellationToken = default)
    {
        var query = db.ActiveUserStatisticSummaries
            .AsNoTrackingWithIdentityResolution()
           .Where(x => x.StatisticsStartTime >= startTime && x.StatisticsStartTime <= endTime)
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
           .Where(x => x.StatisticsTime >= startTime && x.StatisticsTime <= endTime);
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
            .Where(x => startTime <= x.StatisticsTime && x.StatisticsTime <= endTime);

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
             .Where(x => x.StatisticsTime >= startTime && x.StatisticsTime <= endTime);
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

    public Task<StatisticsSmsUsageTrendResponse[]> GetSmsUsageTrendStatisticsAsync(
        DateOnly startTime,
        DateOnly endTime,
        CancellationToken cancellationToken = default)
    {
        var startTime_ = startTime.ToUTC8Date();
        var endTime_ = endTime.AddDays(1).ToUTC8Date();
        return GetSmsUsageTrendStatisticsAsync(startTime_, endTime_, cancellationToken);
    }

    public async Task<StatisticsSmsUsageTrendResponse[]> GetSmsUsageTrendStatisticsAsync(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default)
    {
        var query = db.AuthMessageRecords.Where(r => r.Type == AuthMessageType.PhoneNumber)
            .AsNoTrackingWithIdentityResolution();

        var query2 = query
            .Where(x => x.Type == AuthMessageType.PhoneNumber)
            .Where(x => x.CreateTime >= startTime && x.CreateTime < endTime)
            .GroupBy(x => new
            {
                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(x.CreateTime.UtcDateTime, "Asia/Shanghai").Date, // https://github.com/dotnet/efcore/issues/32340
                x.RequestType
            })
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

        return [];
    }

    public async Task<StatisticsOrderAmountQtyModel[]> GetOrderAmountQtyStatisticsAsync(
        DateOnly startTime,
        DateOnly endTime,
        string? unit,
        PaymentType? paymentType,
        bool taxed,
        bool pureProfit,
        int businessTypeId,
        CancellationToken cancellationToken = default)
    {
        var query = db.OrderAmountQtySummaries
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.BusinessTypeId == businessTypeId)
            .Where(x => x.StatisticsTime >= startTime && x.StatisticsTime <= endTime)
            .OrderBy(x => x.StatisticsTime)
            .Select(a => new StatisticsOrderAmountQtyModel
            {
                Amount = a.Amount,
                Quantity = a.Quantity,
                RefundAmount = a.RefundAmount,
                RefundQuantity = a.RefundQuantity,
                BusinessTypeId = a.BusinessTypeId,
                PaymentType = a.PaymentType,
                StatisticsTime = a.StatisticsTime,
            });

        // 按支付类型过滤
        if (paymentType != null)
            query = query.Where(a => a.PaymentType == paymentType);
#if DEBUG
        var sqlString = query.ToQueryString();
#endif
        var r = await query.ToArrayAsync(cancellationToken);

        var g = GroupByUnit(unit, r);
        var query2 = from m in g
                     let first = m.First()
                     select new StatisticsOrderAmountQtyModel
                     {
                         StatisticsTime = first.StatisticsTime,
                         BusinessTypeId = m.Key.BusinessTypeId,
                         Amount = m.Sum(static x => x.Amount),
                         Quantity = m.Sum(static x => x.Quantity),
                     };
        return [.. query2];
    }

    static int GetWeekOfYear(DateOnly d) =>
        gregorianCalendar.GetWeekOfYear(d.ToDateTime(default), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

    static IEnumerable<IGrouping<(string UnitKey, int BusinessTypeId), StatisticsOrderAmountQtyModel>> GroupByUnit(
        string? unit,
        StatisticsOrderAmountQtyModel[] models) => unit switch
        {
            "day" => models.GroupBy(a => (a.StatisticsTime.ToString(), a.BusinessTypeId)),
            "week" => models.GroupBy(a => (a.StatisticsTime.Year.ToString() + GetWeekOfYear(a.StatisticsTime), a.BusinessTypeId)),
            "month" => models.GroupBy(a => (a.StatisticsTime.Year.ToString() + a.StatisticsTime.Month, a.BusinessTypeId)),
            "year" => models.GroupBy(a => (a.StatisticsTime.Year.ToString(), a.BusinessTypeId)),
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null),
        };

    public Task<OrderAmountQtyTableModel[]> GetOrderSummaryTable(
        DateOnly startTime,
        DateOnly endTime,
        OrderType[]? orderTypes = null,
        int[]? orderBusinessTypeIds = null,
        string? orderBy = null,
        bool? desc = null,
        CancellationToken cancellationToken = default)
    {
        var startTime_ = startTime.ToUTC8Date();
        var endTime_ = endTime.AddDays(1).ToUTC8Date();
        return GetOrderSummaryTable(startTime_, endTime_, orderTypes, orderBusinessTypeIds, orderBy, desc, cancellationToken);
    }

    public async Task<OrderAmountQtyTableModel[]> GetOrderSummaryTable(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        OrderType[]? orderTypes = null,
        int[]? orderBusinessTypeIds = null,
        string? orderBy = null,
        bool? desc = null,
        CancellationToken cancellationToken = default)
    {
        var taxs = await TaxHelper.GetTaxAsync(db, cancellationToken);
        var alipayTax = taxs[TaxHelper.键_支付宝税点];
        var wechatPayTax = taxs[TaxHelper.键_微信支付税点];

        var orders = db.Orders;
        var orderPaymentCompositions = db.OrderPaymentCompositions
             .Where(x =>
                x.PaymentStatus != PaymentStatus.WaitPay &&
                x.PaymentMethod == PaymentMethod.Online);
        var refundBills = db.RefundBills
            .Include(x => x.AftersalesBill)
            .Where(x =>
                x.RefundStatus == RefundStatus.Refund);

        var query = from o in orders
                    join c in orderPaymentCompositions on o.Id equals c.OrderId
                    join r in refundBills on o.Id equals r.AftersalesBill!.OrderId into rs
                    from r in rs.DefaultIfEmpty()
                    where o.PaymentTime >= startTime && o.PaymentTime < endTime
                    select new
                    {
                        o.Type,
                        o.BusinessTypeId,
                        o.Note,
                        c.PaymentAmount,
                        ActualAmount =
                                c.PaymentType == PaymentType.Alipay ? c.PaymentAmount - c.PaymentAmount * alipayTax :
                                c.PaymentType == PaymentType.WeChatPay ? c.PaymentAmount - c.PaymentAmount * wechatPayTax
                                : c.PaymentAmount,
                        RefundAmount = (decimal?)r.RefundAmount ?? 0m,
                    } into row
                    select new
                    {
                        row.Type,
                        row.BusinessTypeId,
                        row.Note,
                        row.PaymentAmount,
                        row.ActualAmount,
                        row.RefundAmount,
                        //Profit = (row.ActualAmount - row.RefundAmount) * xunYouOrderProfitMargin,
                        //Share = (row.PaymentAmount - row.RefundAmount) * xunYouOrderShareRatio,
                    } into row
                    group row by new
                    {
                        row.Type,
                        row.BusinessTypeId,
                    } into g
                    orderby g.Key.Type, g.Key.BusinessTypeId
                    select new OrderAmountQtyTableModel
                    {
                        OrderType = g.Key.Type,
                        OrderBusinessTypeId = g.Key.BusinessTypeId,
                        PaymentCount = g.Count(),
                        RefundCount = g.Count(a => a.RefundAmount > 0),
                        PaymentAmount = g.Sum(o => o.PaymentAmount),
                        ActualAmount = g.Sum(o => o.ActualAmount),
                        RefundAmount = g.Sum(o => o.RefundAmount),
                        //Profit = g.Sum(o => o.Profit),
                        //Share = g.Sum(o => o.Share),
                    };

        if (orderBusinessTypeIds?.Length > 0)
            query = query.Where(a => orderBusinessTypeIds.Contains(a.OrderBusinessTypeId));

        // 结算上月的订单在本月退款
        var lastMonthStartTime = startTime.AddDays(1 - startTime.Day).AddMonths(-1);
        var lastMonthEndTime = startTime.AddDays(1 - startTime.Day);

        var query2 = from o in orders
                     join r in refundBills on o.Id equals r.AftersalesBill!.OrderId into rs
                     from r in rs.DefaultIfEmpty()
                     where o.PaymentTime >= lastMonthStartTime && o.PaymentTime < lastMonthEndTime &&
                           r.RefundFinishTime >= startTime && r.RefundFinishTime < endTime
                     select new
                     {
                         o.Type,
                         o.BusinessTypeId,
                         o.Note,
                         RefundAmount = (decimal?)r.RefundAmount ?? 0m,
                     } into row
                     group row by new
                     {
                         row.Type,
                         row.BusinessTypeId,
                         row.Note,
                     } into g
                     select new OrderAmountQtyTableModel
                     {
                         OrderType = g.Key.Type,
                         OrderBusinessTypeId = g.Key.BusinessTypeId,
                         GoodsType = g.Key.Note,
                         LastMonthRefundCount = g.Count(),
                         LastMonthRefundAmount = g.Sum(x => x.RefundAmount)
                     };
        if (orderBusinessTypeIds?.Length > 0)
            query2 = query2.Where(a => orderBusinessTypeIds.Contains(a.OrderBusinessTypeId));
#if DEBUG
        var sqlString = query.ToQueryString();
        var sqlString2 = query2.ToQueryString();
#endif
        var orderModels = await query.ToArrayAsync(cancellationToken);
        var refundModels = await query2.ToArrayAsync(cancellationToken);

        foreach (var item in orderModels)
        {
            var refund = refundModels.Where(p => p.GoodsType == item.GoodsType).FirstOrDefault();
            if (refund == null)
            {
                continue;
            }
            item.LastMonthRefundAmount = (decimal?)refund.LastMonthRefundAmount ?? 0m;
            //item.Profit = (item.ActualAmount - item.RefundAmount - refund.LastMonthRefundAmount) * xunYouOrderProfitMargin;
            //item.Share = (item.PaymentAmount - item.RefundAmount - refund.LastMonthRefundAmount) * xunYouOrderShareRatio;
            item.LastMonthRefundCount = refund.LastMonthRefundCount;
        }

        var sumOrder = new OrderAmountQtyTableModel()
        {
            GoodsType = "合计",
            PaymentCount = orderModels.Sum(a => a.PaymentCount),
            RefundCount = orderModels.Sum(a => a.RefundCount),
            LastMonthRefundCount = orderModels.Sum(a => a.LastMonthRefundCount),
            LastMonthRefundAmount = orderModels.Sum(a => a.LastMonthRefundAmount),
            PaymentAmount = orderModels.Sum(a => a.PaymentAmount),
            ActualAmount = orderModels.Sum(a => a.ActualAmount),
            RefundAmount = orderModels.Sum(a => a.RefundAmount),
            Profit = orderModels.Sum(a => a.Profit),
            Share = orderModels.Sum(a => a.Share),
        };

        var result = new OrderAmountQtyTableModel[orderModels.Length + 1];
        orderModels.CopyTo(result, 0);
        result[orderModels.Length] = sumOrder;
        return result;
    }

    public async Task<AnalysisResponse[]?> GetStartServiceLogAnalysis(
        DateOnly startTime,
        DateOnly endTime,
        string? appVersion,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        var query = db.AnalysisStartServiceLogSummaries
           .AsNoTrackingWithIdentityResolution()
           .Where(x =>
                x.StatisticsTime >= startTime &&
                x.StatisticsTime <= endTime &&
                x.IsTheMonthlyStatistics == isMonth);
        if (appId.HasValue)
            query = query.Where(x => x.AppId == appId);
        if (!string.IsNullOrWhiteSpace(appVersion))
            query = query.Where(x => x.AppVersion == appVersion);

        var query2 = query.GroupBy(x => x.StatisticsTime)
            .Select(x => new AnalysisResponse
            {
                Date = x.Key,
                StatisticsCount = x.Sum(a => a.StatisticalValues),
                Name = "活跃用户"
            })
            .OrderBy(x => x.Date);
#if DEBUG
        var sqlString = query2.ToQueryString();
#endif
        var r = await query2.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<AnalysisResponse[]?> GetStartSessionLogAnalysis(
        DateOnly startTime,
        DateOnly endTime,
        string? appVersion,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        var query = db.AnalysisStartSessionLogSummaries
                  .AsNoTrackingWithIdentityResolution()
                  .Where(x =>
                    x.StatisticsTime >= startTime &&
                    x.StatisticsTime <= endTime &&
                    x.IsTheMonthlyStatistics == isMonth);
        if (appId.HasValue)
            query = query.Where(x => x.AppId == appId);
        if (!string.IsNullOrWhiteSpace(appVersion))
            query = query.Where(x => x.AppVersion == appVersion);

        var query2 = query.GroupBy(x => x.StatisticsTime)
            .Select(x => new AnalysisResponse
            {
                Date = x.Key,
                StatisticsCount = x.Sum(a => a.StatisticalValues),
                Name = "开始会话"
            })
            .OrderBy(x => x.Date);
#if DEBUG
        var sqlString = query2.ToQueryString();
#endif
        var r = await query2.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<string[]> GetAnalysisEvnetMenuList(
        DateOnly startTime,
        DateOnly endTime,
        string? appVersion,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        var query = db.AnalysisEventLogSummaries
            .AsNoTrackingWithIdentityResolution()
            .Where(x =>
                x.StatisticsTime >= startTime &&
                x.StatisticsTime <= endTime &&
                x.IsTheMonthlyStatistics == isMonth);
        if (appId.HasValue)
            query = query.Where(x => x.AppId == appId);
        if (!string.IsNullOrWhiteSpace(appVersion))
            query = query.Where(x => x.AppVersion == appVersion);

        var query2 = query.OrderBy(x => x.StatisticsTime)
            .Select(x => x.EventName)
            .Distinct();
#if DEBUG
        var sqlString = query2.ToQueryString();
#endif
        var r = await query2.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<string[]?> GetAnalysisPropertiesKeyMenuList(
        DateOnly startTime,
        DateOnly endTime,
        string? appVersion,
        string? eventNames,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        var query = db.AnalysisEventLogSummaries
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.EventRelatedPropertie)
            .Where(x =>
                x.StatisticsTime >= startTime &&
                x.StatisticsTime <= endTime
                && x.IsTheMonthlyStatistics == isMonth
                && x.EventName == eventNames
                && x.EventRelatedPropertie != null);
        if (appId.HasValue)
            query = query.Where(x => x.AppId == appId);
        if (!string.IsNullOrWhiteSpace(appVersion))
            query = query.Where(x => x.AppVersion == appVersion);

        var query2 = query.SelectMany(x => x.EventRelatedPropertie!)
            .Select(x => x.PropertieKey)
            .Distinct();
#if DEBUG
        var sqlString = query2.ToQueryString();
#endif
        var r = await query2.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<string[]?> GetAnalysisPropertiesValueMenuList(
        DateOnly startTime,
        DateOnly endTime,
        string? appVersion,
        string? eventNames,
        string? key,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        var query = db.AnalysisEventLogSummaries
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.EventRelatedPropertie)
            .Where(x =>
                x.StatisticsTime >= startTime &&
                x.StatisticsTime <= endTime
                && x.IsTheMonthlyStatistics == isMonth
                && x.EventName == eventNames
                && x.EventRelatedPropertie != null);
        if (appId.HasValue)
            query = query.Where(x => x.AppId == appId);
        if (!string.IsNullOrWhiteSpace(appVersion))
            query = query.Where(x => x.AppVersion == appVersion);

        var query2 = query.SelectMany(x => x.EventRelatedPropertie!)
            .Where(x => x.PropertieKey == key)
            .Select(x => x.PropertieValue)
            .Distinct();
#if DEBUG
        var sqlString = query2.ToQueryString();
#endif
        var r = await query2.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<AnalysisResponse[]?> GetAnalysisEventSummary(
        DateOnly startTime,
        DateOnly endTime,
        string? appVersion,
        string? eventNames,
        string? key,
        string? value,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        var query = db.AnalysisEventLogSummaries
           .AsNoTrackingWithIdentityResolution()
           .Include(x => x.EventRelatedPropertie)
           .Where(x =>
                x.StatisticsTime >= startTime &&
                x.StatisticsTime <= endTime &&
                x.IsTheMonthlyStatistics == isMonth &&
                x.EventRelatedPropertie != null);
        if (appId.HasValue)
            query = query.Where(x => x.AppId == appId);
        if (!string.IsNullOrEmpty(eventNames))
            query = query.Where(x => x.EventName == eventNames);
        if (!string.IsNullOrEmpty(appVersion))
            query = query.Where(x => x.AppVersion == appVersion);

        IQueryable<AnalysisResponse> query2;
        IQueryable<AnalysisResponse>? query3 = null;
        if (!string.IsNullOrEmpty(key))
        {
            if (string.IsNullOrEmpty(value))
            {
                query2 = query
                    .SelectMany(x => x.EventRelatedPropertie!.Select(p => new
                    {
                        x.StatisticsTime,
                        p.PropertieKey,
                        p.PropertieValue,
                        p.StatisticalValues
                    }))
                    .Where(x => x.PropertieKey == key)
                    .GroupBy(x => new { x.StatisticsTime, x.PropertieValue })
                    .Select(g => new AnalysisResponse
                    {
                        Date = g.Key.StatisticsTime,
                        StatisticsCount = g.Sum(x => x.StatisticalValues),
                        Name = g.Key.PropertieValue
                    })
                    .OrderByDescending(x => x.StatisticsCount);
            }
            else
            {
                query2 = query
                    .GroupBy(x => x.StatisticsTime)
                    .Select(g => new AnalysisResponse
                    {
                        Date = g.Key,
                        StatisticsCount = g.Sum(x => x.EventRelatedPropertie!
                           .Where(x => (string.IsNullOrEmpty(key) || x.PropertieKey == key)
                            && (string.IsNullOrEmpty(value) || x.PropertieValue == value)).Select(x => x.StatisticalValues).Sum()),
                        Name = value ?? key
                    })
                    .OrderByDescending(x => x.StatisticsCount);
            }
        }
        else if (string.IsNullOrEmpty(eventNames))
        {
            query2 = EventStatistics();
        }
        else
        {
            query2 = query
                .SelectMany(x => x.EventRelatedPropertie!.Select(p => new
                {
                    x.StatisticsTime,
                    p.PropertieKey,
                    p.PropertieValue,
                    p.StatisticalValues
                }))
                .GroupBy(x => new { x.StatisticsTime, x.PropertieKey })
                .Select(g => new AnalysisResponse
                {
                    Date = g.Key.StatisticsTime,
                    StatisticsCount = g.Sum(x => x.StatisticalValues),
                    Name = g.Key.PropertieKey
                })
                .OrderByDescending(x => x.StatisticsCount);
            query3 = EventStatistics();
        }
#if DEBUG
        var sqlString = query2.ToQueryString();
        var sqlString3 = query3?.ToQueryString();
#endif

        var r = await query2.ToArrayAsync(cancellationToken);
        if (query3 != null && r.Length == 0)
        {
            r = await query3.ToArrayAsync(cancellationToken);
        }
        return r;

        IQueryable<AnalysisResponse> EventStatistics()
        {
            var query2 = query
                .GroupBy(x => new { x.StatisticsTime, x.EventName })
                .Select(g => new AnalysisResponse
                {
                    Date = g.Key.StatisticsTime,
                    StatisticsCount = g.Sum(x => x.StatisticalValues),
                    Name = g.Key.EventName
                })
                .OrderByDescending(x => x.StatisticsCount);
            return query2;
        }
    }

    public async Task<string[]?> GetAnalysisAppVer(
        CancellationToken cancellationToken = default)
    {
        var query = db.AnalysisStartServiceLogSummaries
            .AsNoTrackingWithIdentityResolution()
            .Select(x => x.AppVersion)
            .Distinct();
#if DEBUG
        var sqlString = query.ToQueryString();
#endif
        var r = await query.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<AnalysisResponse[]?> GetAnalysisAppVerSummary(
        DateOnly startTime,
        DateOnly endTime,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        var query = db.AnalysisStartServiceLogSummaries
            .AsNoTrackingWithIdentityResolution()
            .Where(x =>
                x.StatisticsTime >= startTime &&
                x.StatisticsTime <= endTime &&
                x.IsTheMonthlyStatistics == isMonth);
        if (appId.HasValue)
            query = query.Where(x => x.AppId == appId);

        var query2 = query.GroupBy(x => new { x.StatisticsTime, x.AppVersion })
            .Select(x => new AnalysisResponse
            {
                Date = x.Key.StatisticsTime,
                Name = x.Key.AppVersion,
                StatisticsCount = x.Sum(a => a.StatisticalValues),
            })
            .OrderByDescending(x => x.StatisticsCount);
#if DEBUG
        var sqlString = query2.ToQueryString();
#endif
        var r = await query2.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<AnalysisResponse[]?> GetAnalysisLocaleSummary(
        DateOnly startTime,
        DateOnly endTime,
        bool isall,
        string? appVersion,
        Guid? appId,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        var query = db.ActiveUserLanguageSummaries
           .AsNoTrackingWithIdentityResolution()
           .Where(x =>
                x.StatisticsTime >= startTime &&
                x.StatisticsTime <= endTime &&
                x.IsTheMonthlyStatistics == isMonth);
        if (appId.HasValue)
            query = query.Where(x => x.AppId == appId);

        if (!string.IsNullOrWhiteSpace(appVersion))
            query = query.Where(x => x.AppVersion == appVersion);

        var query2 = query.GroupBy(x => new { x.Locale })
            .Select(x => new AnalysisResponse
            {
                //Date = x.Key.StatisticalTime,
                Name = string.IsNullOrEmpty(x.Key.Locale) ? "未知" : x.Key.Locale,
                StatisticsCount = x.Sum(a => a.StatisticalValues),
            })
            .OrderByDescending(x => x.StatisticsCount)
            .Take(isall ? 1000 : 10);
#if DEBUG
        var sqlString = query2.ToQueryString();
#endif
        var r = await query2.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<AnalysisResponse[]?> GetAnalysisEquipmentSummary(
        DateOnly startTime,
        DateOnly endTime,
        string? appVersion,
        bool isMonth,
        CancellationToken cancellationToken = default)
    {
        var query = db.ActiveUserDeviceSummaries
             .AsNoTrackingWithIdentityResolution()
             .Where(x =>
                x.StatisticsTime >= startTime &&
                x.StatisticsTime <= endTime &&
                x.IsTheMonthlyStatistics == isMonth);

        if (!string.IsNullOrWhiteSpace(appVersion))
            query = query.Where(x => x.AppVersion == appVersion);

        var query2 = query.GroupBy(x => new { x.OsName })
            .Select(x => new AnalysisResponse
            {
                //Date = x.Key.StatisticalTime,
                Name = x.Key.OsName,
                StatisticsCount = x.Sum(a => a.StatisticalValues)
            })
            .OrderByDescending(x => x.StatisticsCount);
#if DEBUG
        var sqlString = query2.ToQueryString();
#endif
        var r = await query2.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<AnalysisResponse[]?> GetAppVerStatisticsData(
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default)
    {
        var query = db.ActiveUserAppVerSummaries
            .AsNoTrackingWithIdentityResolution()
            .Where(x =>
                x.StatisticsTime >= start &&
                x.StatisticsTime <= end);

        var query2 = query.GroupBy(x => new { x.StatisticsTime, x.AppVersion })
            .Select(x => new AnalysisResponse
            {
                Date = x.Key.StatisticsTime,
                Name = x.Key.AppVersion,
                StatisticsCount = x.Sum(a => a.Count)
            })
            .OrderByDescending(x => x.StatisticsCount);
#if DEBUG
        var sqlString = query2.ToQueryString();
#endif
        var r = await query2.ToArrayAsync(cancellationToken);
        return r;
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

    public async Task<int> StatisticsDaysData(DateOnly start, DateOnly end,
        CancellationToken cancellationToken = default)
    {
        var any = await db.ActiveUserStatisticSummaries
            .AnyAsync(x => x.StatisticsStartTime == start && x.StatisticsEndTime == end, cancellationToken);

        // 存在记录跳过此统计数据
        if (!any)
        {
            var start_ = start.ToUTC8Date();
            var end_ = end.ToUTC8Date();

            SetCommandTimeout();

            var registerUserCount = await db.Users
                .AsNoTrackingWithIdentityResolution()
                .CountAsync(x => x.CreateTime >= start_ && x.CreateTime < end_, cancellationToken);

            var query = db.ActiveUserRecords
               .AsNoTrackingWithIdentityResolution()
               .Where(x => x.CreateTime >= start_ && x.CreateTime < end_);
#if DEBUG
            var sqlString = query.ToQueryString();
#endif

            // 总记录数
            var count = await query.CountAsync(cancellationToken);
            if (count == 0 && registerUserCount == 0)
            {
                return 0;
            }

            // 登录数量
            var loginCountQ = query.Where(x => x.IsAuthenticated == true);
#if DEBUG
            var sqlString_loginCountQ = loginCountQ.ToQueryString();
#endif
            var loginCount = await loginCountQ.CountAsync(cancellationToken);


            // IP 计数 Distinct 去重
            var ipCountQ = query
                .Select(p => p.IPAddress)
                .Distinct();
#if DEBUG
            var sqlString_ipCountQ = ipCountQ.ToQueryString();
#endif
            var ipCount = await ipCountQ.CountAsync(cancellationToken);


            var deviceIdCountQ = query
                .Where(x => x.DeviceId != null);
#if DEBUG
            var sqlString_deviceIdCountQ = deviceIdCountQ.ToQueryString();
#endif
            var deviceIdCount = await deviceIdCountQ.CountAsync(cancellationToken);

            // 平台分布数量
            var platformsQ = query.GroupBy(x => new
            {
                x.Platform,
                x.DeviceIdiom,
            }).Select(x => new ActiveUserPlatformSummary
            {
                Platform = x.Key.Platform,
                DeviceIdiom = x.Key.DeviceIdiom,
                StatisticsTime = start,
                Count = x.Count(),
            });
#if DEBUG
            var sqlString_platformsQ = platformsQ.ToQueryString();
#endif
            var platforms = await platformsQ.ToListAsync(cancellationToken);

            // 设备 CPU 架构
            var archsQ = query.GroupBy(x => new
            {
                x.ProcessArch,
                x.Platform,
                x.DeviceIdiom,
            }).Select(x => new ActiveUserArchitectureSummary
            {
                Platform = x.Key.Platform,
                DeviceIdiom = x.Key.DeviceIdiom,
                ProcessArch = x.Key.ProcessArch,
                StatisticsTime = start,
                Count = x.Count(),
            });
#if DEBUG
            var sqlString_archsQ = archsQ.ToQueryString();
#endif
            var archs = await archsQ.ToListAsync(cancellationToken);

            var osVersQ = query.GroupBy(x => new
            {
                x.OSVersion,
                x.Platform,
                x.DeviceIdiom,
            }).Select(x => new ActiveUserOSSummary
            {
                Platform = x.Key.Platform,
                DeviceIdiom = x.Key.DeviceIdiom,
                OSVersion = x.Key.OSVersion,
                StatisticsTime = start,
                Count = x.Count(),
            });
#if DEBUG
            var sqlString_osVersQ = osVersQ.ToQueryString();
#endif
            var osVers = await osVersQ.ToListAsync(cancellationToken);
            for (int i = 0; i < osVers.Count; i++)
            {
                var it = osVers[i];
                if (!string.IsNullOrWhiteSpace(it.OSVersion) && it.OSVersion.AsSpan().TryGetVersion(out var osVersion))
                {
                    it.SetOSVersionSections(osVersion);
                }
            }

            var summary = new ActiveUserStatisticSummary
            {
                StatisticsStartTime = start,
                StatisticsEndTime = end,
                IPCount = ipCount,
                DeviceIdCount = deviceIdCount,
                AllCount = count,
                LoginCount = loginCount,
                RegisterUserCount = registerUserCount,
                Platforms = platforms,
                Architectures = archs,
                OSVersions = osVers,
            };

            await db.ActiveUserStatisticSummaries.AddAsync(summary, cancellationToken);
            var rowCount = await db.SaveChangesAsync(cancellationToken);
            return rowCount;
        }

        return 0;
    }

    public async Task<int> DeleteDaysData(DateTimeOffset start, DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        SetCommandTimeout();
        var query = db.ActiveUserRecords
            .Where(x => x.CreateTime >= start && x.CreateTime < end);
#if DEBUG
        var sqlString = query.ToQueryString();
#endif
        var r = await query.ExecuteDeleteAsync(cancellationToken);
        return r;
    }

    public async Task<int> StatisticsAppVerDaysData(DateOnly start, DateOnly end,
        CancellationToken cancellationToken = default)
    {
        SetCommandTimeout();
        var query = db.ActiveUserAppVerSummaries
            .Where(x => x.StatisticsTime == start);
#if DEBUG
        var sqlString = query.ToQueryString();
#endif
        var any = await query.AnyAsync(cancellationToken);
        if (!any)
        {
            var start_ = start.ToUTC8Date();
            var end_ = end.ToUTC8Date();

            var query2 = db.ActiveUserRecords
                .AsNoTrackingWithIdentityResolution()
                .Where(x => x.CreateTime >= start_ && x.CreateTime < end_)
                .GroupBy(x => x.AppVersion)
                .Select(x => new ActiveUserAppVerSummary
                {
                    AppVersion = x.Key,
                    Count = x.Count(),
                    StatisticsTime = start,
                    CreateTime = DateTimeOffset.Now,
                });
#if DEBUG
            var sqlString2 = query2.ToQueryString();
#endif
            var summaries = await query2.ToArrayAsync(cancellationToken);
            await db.ActiveUserAppVerSummaries.AddRangeAsync(summaries, cancellationToken);
            var rowCount = await db.SaveChangesAsync(cancellationToken);
            return rowCount;
        }
        return 0;
    }

    public async Task<KomaasharuStatisticPerDaySummary?> GetLastAdvertisementStatisticDaily(
        CancellationToken cancellationToken = default)
    {
        var query = db.KomaasharuStatisticPerDaySummaries
            .AsNoTrackingWithIdentityResolution()
            .OrderByDescending(a => a.StatisticsTime)
            .Take(1);
#if DEBUG
        var sqlString = query.ToQueryString();
#endif
        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<KomaasharuStatistic?> GetFirstAdvertisementStatistic(
        CancellationToken cancellationToken = default)
    {
        var query = db.KomaasharuStatistics
            .AsNoTrackingWithIdentityResolution()
            .OrderBy(a => a.StatisticsTime)
            .Take(1);
#if DEBUG
        var sqlString = query.ToQueryString();
#endif
        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<int> PerformAdvertisementStatisticDaily(DateOnly statisticDate,
        CancellationToken cancellationToken = default)
    {
        SetCommandTimeout();

        var query = db.KomaasharuStatisticPerDaySummaries
            .Where(x => x.StatisticsTime == statisticDate);
        var any = await query.AnyAsync(cancellationToken);
        if (any)
        {
            // 存在记录跳过此统计
            return 0;
        }

        var start = statisticDate.ToUTC8Date();
        var end = start.AddDays(1);

        var query2 = db.KomaasharuStatistics
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.CreateTime >= start && x.CreateTime < end);

        var query3 = query2
            .GroupBy(a => new { a.KomaasharuId, a.Platform, a.DeviceIdiom })
            .Select(g => new KomaasharuStatisticPerDaySummary
            {
                KomaasharuId = g.Key.KomaasharuId,
                Platform = g.Key.Platform,
                DeviceIdiom = g.Key.DeviceIdiom,
                NumDisplay = g.Sum(a => a.NumDisplay),
                NumClick = g.Sum(a => a.NumClick),
                StatisticsTime = statisticDate,
            });
#if DEBUG
        var sqlString3 = query3.ToQueryString();
#endif
        var summaries = await query3.ToArrayAsync(cancellationToken);
        await db.KomaasharuStatisticPerDaySummaries.AddRangeAsync(summaries, cancellationToken);
        var rowCount = await db.SaveChangesAsync(cancellationToken);
        return rowCount;
    }

    public async Task<ActiveUserDayWeekMonthSummary?> GetLastUserDWMStatisticDaily(
        CancellationToken cancellationToken = default)
    {
        var query = db.ActiveUserDayWeekMonthSummaries
            .AsNoTrackingWithIdentityResolution()
            .OrderByDescending(a => a.StatisticsTime)
            .Take(1);
#if DEBUG
        var sqlString = query.ToQueryString();
#endif
        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<int> PerformUserDWMStatisticDaily(DateOnly statisticDate,
        CancellationToken cancellationToken = default)
    {
        SetCommandTimeout();

        var query = db.ActiveUserDayWeekMonthSummaries
            .Where(x => x.StatisticsTime == statisticDate);
        var any = await query.AnyAsync(cancellationToken);
        if (any)
        {
            // 存在记录跳过此统计
            return 0;
        }

        var startTime = statisticDate.ToUTC8Date();
        var endTime = startTime.AddDays(1);

        ActiveUserStatisticsType?[] statisticsTypes =
        [
            ActiveUserStatisticsType.AU_1,
            ActiveUserStatisticsType.AU_7,
            ActiveUserStatisticsType.AU_30,
            endTime.Day == 1? ActiveUserStatisticsType.AU_Month:null
        ];

        for (int i = 0; i < statisticsTypes.Length; i++)
        {
            var it = statisticsTypes[i];
            if (it.HasValue)
            {
                var summaries = await GetUserActivityStatisticAsync(statisticDate, endTime, it.Value, cancellationToken);
                await db.ActiveUserDayWeekMonthSummaries.AddRangeAsync(summaries, cancellationToken);
            }
        }

        var rowCount = await db.SaveChangesAsync(cancellationToken);
        return rowCount;
    }

    async Task<ActiveUserDayWeekMonthSummary[]> GetUserActivityStatisticAsync(
        DateOnly statisticDate,
        DateTimeOffset endTime,
        ActiveUserStatisticsType auType,
        CancellationToken cancellationToken = default)
    {
        var startTime = auType switch
        {
            ActiveUserStatisticsType.AU_1 => endTime.AddDays(-1),
            ActiveUserStatisticsType.AU_7 => endTime.AddDays(-7),
            ActiveUserStatisticsType.AU_30 => endTime.AddDays(-30),
            ActiveUserStatisticsType.AU_Month => endTime.AddMonths(-1),
            _ => throw new ArgumentOutOfRangeException(nameof(auType), auType, null),
        };

        var query = db.ActiveUserRecords
            .AsNoTrackingWithIdentityResolution()
            .Where(x =>
                x.DeviceId != null &&
                x.CreateTime >= startTime && x.CreateTime < endTime)
            .GroupBy(r => new { r.Platform, r.DeviceIdiom })
            .Select(g => new ActiveUserDayWeekMonthSummary
            {
                Platform = g.Key.Platform,
                DeviceIdiom = g.Key.DeviceIdiom,
                ActiveUserCount = g.Select(r => r.DeviceId).Distinct().Count(), //  以设备 Id 去重后的用户数量
            });
#if DEBUG
        var sqlString = query.ToQueryString();
#endif
        var r = await query.ToArrayAsync(cancellationToken);
        var startTime_ = startTime.GetDateOnly();
        var endTime_ = endTime.GetDateOnly();
        for (int i = 0; i < r.Length; i++)
        {
            var it = r[i];
            it.AUType = auType;
            it.StatisticsTime = statisticDate;
            it.StatisticsStartTime = startTime_;
            it.StatisticsEndTime = endTime_;
        }
        return r;
    }

    public async Task<OrderAmountQtySummary?> GetLastOrderAmountQtySummary(
        CancellationToken cancellationToken = default)
    {
        var query = db.OrderAmountQtySummaries
            .AsNoTrackingWithIdentityResolution()
            .OrderByDescending(a => a.StatisticsTime)
            .Take(1);
#if DEBUG
        var sqlString = query.ToQueryString();
#endif
        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<int> PerformOrderAmountQtyStatisticDaily(DateOnly statisticDate,
        CancellationToken cancellationToken = default)
    {
        SetCommandTimeout();

        var query = db.OrderAmountQtySummaries
            .Where(x => x.StatisticsTime == statisticDate);
        var any = await query.AnyAsync(cancellationToken);
        if (any)
        {
            // 存在记录跳过此统计
            return 0;
        }

        var startTime = statisticDate.ToUTC8Date();
        var endTime = startTime.AddDays(1);

        var query1 = from order in db.Orders
                     join payment in db.OrderPaymentCompositions on order.Id equals payment.OrderId
                     where order.PaymentTime >= startTime &&
                          order.PaymentTime < endTime &&
                          payment.PaymentStatus != PaymentStatus.WaitPay &&
                          payment.PaymentMethod == PaymentMethod.Online
                     group new { order, payment } by new
                     {
                         order.BusinessTypeId,
                         payment.PaymentType,
                     }
                     into groupItem
                     select new OrderAmountQtySummary
                     {
                         Amount = groupItem.Sum(a => a.payment.PaymentAmount),
                         Quantity = groupItem.Count(),
                         BusinessTypeId = groupItem.Key.BusinessTypeId,
                         PaymentType = groupItem.Key.PaymentType,
                         StatisticsTime = statisticDate,
                     };

        var query2 = from order in db.Orders
                     join payment in db.OrderPaymentCompositions on order.Id equals payment.OrderId
                     join refund in db.RefundBills on order.Id equals refund.AftersalesBill!.OrderId
                     where refund.RefundFinishTime >= endTime.AddDays(-1) &&
                           refund.RefundFinishTime < endTime &&
                           payment.PaymentStatus != PaymentStatus.WaitPay &&
                           payment.PaymentMethod == PaymentMethod.Online
                     group new { order, payment, refund } by new
                     {
                         order.BusinessTypeId,
                         payment.PaymentType,
                     }
                     into groupItem
                     select new OrderAmountQtySummary
                     {
                         RefundAmount = groupItem.Sum(a => a.refund.RefundAmount),
                         RefundQuantity = groupItem.Count(),
                         BusinessTypeId = groupItem.Key.BusinessTypeId,
                         PaymentType = groupItem.Key.PaymentType,
                         StatisticsTime = statisticDate,
                     };
#if DEBUG
        var sqlString1 = query1.ToQueryString();
        var sqlString2 = query2.ToQueryString();
#endif

        var summaries1 = await query1.ToArrayAsync(cancellationToken);
        var summaries2 = await query2.ToArrayAsync(cancellationToken);

        var summaries = summaries1.Union(summaries2).GroupBy(a => new
        {
            a.BusinessTypeId,
            a.PaymentType,
            a.StatisticsTime,
        }).Select(g => new OrderAmountQtySummary
        {
            BusinessTypeId = g.Key.BusinessTypeId,
            PaymentType = g.Key.PaymentType,
            StatisticsTime = g.Key.StatisticsTime,
            Amount = g.Sum(a => a.Amount),
            Quantity = g.Sum(a => a.Quantity),
            RefundAmount = g.Sum(a => a.RefundAmount),
            RefundQuantity = g.Sum(a => a.RefundQuantity),
        }).ToArray();

        await db.OrderAmountQtySummaries.AddRangeAsync(summaries, cancellationToken);
        var rowCount = await db.SaveChangesAsync(cancellationToken);
        return rowCount;
    }
}

static partial class TaxHelper
{
    public const string 键_支付宝税点 = "AlipayTax";
    public const string 键_微信支付税点 = "WechatPayTax";

    static decimal GetTaxDef(string key) => key switch
    {
        键_支付宝税点 => 0.01m,
        键_微信支付税点 => 0.006m,
        _ => default,
    };

    public static async Task<Dictionary<string, decimal>> GetTaxAsync(
        IKeyValuePairsDbContext db,
        CancellationToken cancellationToken = default)
    {
        string[] keys =
        [
            键_支付宝税点,
            键_微信支付税点,
        ];

        var query = db.KeyValuePairs
            .Where(x => x.DeleteTime == null && keys.Contains(x.Id))
            .Select(static x => new
            {
                x.Id,
                x.Value,
            });
#if DEBUG
        var sqlString = query.ToQueryString();
#endif
        var r = await query.ToArrayAsync(cancellationToken);

        var query2 = from m in r
                     let tax = decimal.TryParse(m.Value, out var number) ? number : GetTaxDef(m.Id)
                     where tax != default
                     select KeyValuePair.Create(m.Id, tax);
        Dictionary<string, decimal> r2 = new(query2);
        for (int i = 0; i < keys.Length; i++)
        {
            var it = keys[i];
            if (!r2.ContainsKey(it))
            {
                r2.Add(it, GetTaxDef(it));
            }
        }
        return r2;
    }
}

file static class LazyStatisticUserCountDict
{
    /// <summary>
    /// <see cref="ExternalLoginChannel"/> 到 <see cref="StatisticUserCount"/> 的字典
    /// </summary>
    internal static readonly Dictionary<ExternalLoginChannel, StatisticUserCount> dictFLCToSUC = new()
    {
        { ExternalLoginChannel.Steam, StatisticUserCount.BindSteam },
        { ExternalLoginChannel.QQ, StatisticUserCount.BindQQ },
        { ExternalLoginChannel.Microsoft, StatisticUserCount.BindMS },
        { ExternalLoginChannel.Alipay, StatisticUserCount.BindAlipay },
        { ExternalLoginChannel.Weixin, StatisticUserCount.BindWeChat },
    };
}

file static class LazyCalendar
{
    internal static readonly Calendar gregorianCalendar = new CultureInfo("zh-CN").Calendar;
}