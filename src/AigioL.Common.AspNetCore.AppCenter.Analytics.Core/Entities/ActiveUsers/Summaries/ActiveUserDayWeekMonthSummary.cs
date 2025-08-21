using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers.Summaries;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers.Summaries;

/// <summary>
/// 活跃用户的日活、周活、月活汇总
/// </summary>
[Table("ActiveUserDayWeekMonthSummaries")]
public partial class ActiveUserDayWeekMonthSummary :
    Entity<Guid>,
    ICreationTime,
    INEWSEQUENTIALID
{
    public ActiveUserStatisticsType AUType { get; set; }

    /// <summary>
    /// 平台
    /// </summary>
    [Comment("平台")]
#pragma warning disable CS0618 // 类型或成员已过时
    public WebApiCompatDevicePlatform Platform { get; set; }
#pragma warning restore CS0618 // 类型或成员已过时

    /// <summary>
    /// 设备类型
    /// </summary>
    [Comment("设备类型")]
    public DeviceIdiom DeviceIdiom { get; set; }

    /// <summary>
    /// 用户数量
    /// </summary>
    public int ActiveUserCount { get; set; }

    /// <summary>
    /// 被统计数据的开始时间
    /// </summary>
    public DateTimeOffset StatisticsStartTime { get; set; }

    /// <summary>
    /// 被统计数据的结束时间
    /// </summary>
    public DateTimeOffset StatisticsEndTime { get; set; }

    /// <summary>
    /// 统计日期
    /// </summary>
    public DateTimeOffset StatisticsTime { get; set; }

    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }

    public static Expression<Func<ActiveUserDayWeekMonthSummary, UserActivityStatisticsResponse>> Expression => _.Expression;
}

file static class _
{
    internal static readonly Expression<Func<ActiveUserDayWeekMonthSummary, UserActivityStatisticsResponse>> Expression = x => new()
    {
        AUType = x.AUType,
        Platform = x.Platform,
        DeviceIdiom = x.DeviceIdiom,
        Count = x.ActiveUserCount,
        StatisticsTime = x.StatisticsTime,
    };
}