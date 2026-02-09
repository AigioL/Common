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
using GameTrainer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using static AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.Abstractions.IStatisticsRepository;
using static AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.LazyCalendar;
using static AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.LazyStatisticUserCountDict;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            .Where(x => x.StatisticsTime >= startTime && x.StatisticsTime < endTime)
            .OrderBy(x => x.StatisticsTime)
            .Select(a => new StatisticsOrderAmountQtyModel
            {
                Amount = a.Amount,
                Quantity = a.Quantity,
                RefundAmount = a.RefundAmount,
                RefundQuantity = a.RefundQuantity,
                BusinessTypeId = a.BusinessTypeId,
                GoodsType = a.GoodsType,
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
                         BusinessTypeId = first.BusinessTypeId,
                         Amount = m.Sum(static x => x.Amount),
                         Quantity = m.Sum(static x => x.Quantity),
                         GoodsType = first.GoodsType,
                     };
        return [.. query2];
    }

    static int GetWeekOfYear(DateOnly d) =>
        gregorianCalendar.GetWeekOfYear(d.ToDateTime(default), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

    static IEnumerable<IGrouping<(string UnitKey, string GoodsType), StatisticsOrderAmountQtyModel>> GroupByUnit(
        string? unit,
        StatisticsOrderAmountQtyModel[] models) => unit switch
        {
            "day" => models.GroupBy(a => (a.StatisticsTime.ToString(), a.GoodsType)),
            "week" => models.GroupBy(a => (a.StatisticsTime.Year.ToString() + GetWeekOfYear(a.StatisticsTime), a.GoodsType)),
            "month" => models.GroupBy(a => (a.StatisticsTime.Year.ToString() + a.StatisticsTime.Month, a.GoodsType)),
            "year" => models.GroupBy(a => (a.StatisticsTime.Year.ToString(), a.GoodsType)),
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null),
        };

    public async Task<OrderAmountQtyTableModel[]> GetOrderSummaryTable(
        DateOnly startTime,
        DateOnly endTime,
        OrderType[]? orderTypes = null,
        int[]? orderBusinessTypeIds = null,
        string? orderBy = null,
        bool? desc = null,
        CancellationToken cancellationToken = default)
    {
        var taxs = await TaxHelper.GetTaxAsync(db, cancellationToken);
        var alipayTax = taxs[TaxHelper.键_支付宝税点];
        var wechatPayTax = taxs[TaxHelper.键_微信支付税点];

        var startTime_ = ToUTC8Date(startTime);
        var endTime_ = ToUTC8Date(startTime);

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
                    where o.PaymentTime >= startTime_ && o.PaymentTime < endTime_
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
                        row.Note,
                    } into g
                    orderby g.Key.Type, g.Key.BusinessTypeId, g.Key.Note
                    select new OrderAmountQtyTableModel
                    {
                        OrderType = g.Key.Type,
                        OrderBusinessTypeId = g.Key.BusinessTypeId,
                        GoodsType = g.Key.Note,
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
        var lastMonthStartTime_ = ToUTC8Date(lastMonthStartTime);
        var lastMonthEndTime = startTime.AddDays(1 - startTime.Day);
        var lastMonthEndTime_ = ToUTC8Date(lastMonthEndTime);

        var query2 = from o in orders
                     join r in refundBills on o.Id equals r.AftersalesBill!.OrderId into rs
                     from r in rs.DefaultIfEmpty()
                     where o.PaymentTime >= lastMonthStartTime_ && o.PaymentTime < lastMonthEndTime_ &&
                           r.RefundFinishTime >= startTime_ && r.RefundFinishTime < endTime_
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