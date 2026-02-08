using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers.Summaries;

[Table("ActiveUserScreenResolutionSummaries")]
public sealed class ActiveUserScreenResolutionSummary :
    Entity<Guid>,
    INEWSEQUENTIALID,
    ICreateTime
{
    public Guid StatisticsId { get; set; }

    public int ScreenCount { get; set; }

    public double PrimaryScreenPixelDensity { get; set; }

    public int PrimaryScreenWidth { get; set; }

    public int PrimaryScreenHeight { get; set; }

    public int SumScreenWidth { get; set; }

    public int SumScreenHeight { get; set; }

    public int Count { get; set; }

    /// <summary>
    /// 统计日期
    /// </summary>
    [Comment("统计日期")]
    public DateOnly StatisticsTime { get; set; }

    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreateTime { get; set; }
}
