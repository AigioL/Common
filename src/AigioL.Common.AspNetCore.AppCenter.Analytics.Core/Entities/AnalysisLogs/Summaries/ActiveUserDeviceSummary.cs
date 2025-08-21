using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs.Summaries;

/// <summary>
/// 活跃用户设备汇总表实体类
/// </summary>
[Table("ActiveUserDeviceSummaries")]
public partial class ActiveUserDeviceSummary :
    Entity<Guid>,
    ICreationTime,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 操作系统名称
    /// </summary>
    [Required]
    [Comment("操作系统名称")]
    public required string OsName { get; set; }

    /// <summary>
    /// 应用程序版本号
    /// </summary>
    [Required]
    [Comment("应用程序版本号")]
    public required string AppVersion { get; set; }

    /// <summary>
    /// 统计值
    /// </summary>
    [Comment("统计值")]
    public int StatisticalValues { get; set; }

    /// <summary>
    /// 统计时间
    /// </summary>
    [Comment("统计时间")]
    public DateTimeOffset StatisticalTime { get; set; }

    /// <summary>
    /// 应用 Id
    /// </summary>
    [Comment("应用 Id")]
    public Guid AppId { get; set; }

    /// <summary>
    /// 是否月统计
    /// </summary>
    [Comment("是否月统计")]
    public bool IsTheMonthlyStatistics { get; set; }

    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }
}