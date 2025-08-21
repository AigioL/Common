using AigioL.Common.Primitives.Entities.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers.Summaries;

[Table("ActiveUserScreenResolutionSummaries")]
public sealed class ActiveUserScreenResolutionSummary :
    Entity<Guid>,
    INEWSEQUENTIALID
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
    /// 统计日期（当天的数据）
    /// </summary>
    public DateTimeOffset StatisticsTime { get; set; }
}
