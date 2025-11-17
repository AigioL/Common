using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus.Summaries;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AppCenter.Entities.Komaasharus.Summaries;

/// <summary>
/// 广告统计每日汇总表实体类
/// </summary>
[Table("AdvertisementStatisticPerDaySummaries")]
public partial class KomaasharuStatisticPerDaySummary :
    Entity<Guid>,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 广告 Id
    /// </summary>
    [Comment("广告 Id")]
    public Guid KomaasharuId { get; set; }

    public virtual Komaasharu Komaasharu { get; set; } = null!;

    /// <summary>
    /// 点击数
    /// </summary>
    [Comment("点击数")]
    public long NumClick { get; set; }

    /// <summary>
    /// 展示数
    /// </summary>
    [Comment("展示数")]
    public long NumDisplay { get; set; }

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
    /// 统计日期（当天的数据）
    /// </summary>
    public DateTimeOffset StatisticsTime { get; set; }

    public static Expression<Func<KomaasharuStatisticPerDaySummary, StatisticsKomaasharuResponse>> Expression => _Expression.Expression;
}

file static class _Expression
{
    internal static readonly Expression<Func<KomaasharuStatisticPerDaySummary, StatisticsKomaasharuResponse>> Expression = x => new()
    {
        ViewCount = x.NumDisplay,
        ClickCount = x.NumClick,
        StatisticsTime = x.StatisticsTime,
    };
}