using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers.Summaries;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers.Summaries;

/// <summary>
/// 活跃用户平台汇总
/// </summary>
[Table("ActiveUserPlatformSummaries")]
public partial class ActiveUserPlatformSummary :
    Entity<Guid>,
    INEWSEQUENTIALID
{
    public Guid StatisticsId { get; set; }

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
    /// 数量
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 统计日期（当天的数据）
    /// </summary>
    public DateTimeOffset StatisticsTime { get; set; }

    public static Expression<Func<ActiveUserPlatformSummary, StatisticsActiveUserOSResponse>> Expression => _.Expression;
}

file static class _
{
    internal static readonly Expression<Func<ActiveUserPlatformSummary, StatisticsActiveUserOSResponse>> Expression = x => new()
    {
        Platform = x.Platform,
        DeviceIdiom = x.DeviceIdiom,
        Count = x.Count,
        StatisticsTime = x.StatisticsTime,
    };
}