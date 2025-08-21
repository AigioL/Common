using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers.Summaries;

/// <summary>
/// 活跃用户版本统计
/// </summary>
[Table("ActiveUserAppVerSummaries")]
public partial class ActiveUserAppVerSummary :
    Entity<Guid>,
    INEWSEQUENTIALID,
    ICreationTime
{
    public string? AppVersion { get; set; }

    public long Count { get; set; }

    /// <summary>
    /// 统计日期（当天的数据）
    /// </summary>
    public DateTimeOffset StatisticsTime { get; set; }

    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }
}
