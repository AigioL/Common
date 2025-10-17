using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers.Summaries;

/// <summary>
/// 活跃用户系统汇总
/// </summary>
[Table("ActiveUserOSSummaries")]
public partial class ActiveUserOSSummary :
    Entity<Guid>,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 活跃用户统计汇总 Id
    /// </summary>
    [Comment("活跃用户统计汇总 Id")]
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
    /// 系统版本
    /// <para> 通常是由一个3或4位数字的序列组成：
    /// <list type="bullet">
    ///     <item> major.minor[.build[.revision]] (example: 1.2.12.102) </item>
    ///     <item> major.minor[.maintenance[.build]] (example: 1.4.3.5249) </item>
    /// </list>
    /// </para>
    /// </summary>
    [Required]
    [Comment("系统版本")]
    [StringLength(MaxLengths.Version)]
    public required string OSVersion { get; set; }

    public int OSVersionNumber { get; set; }

    /// <summary>
    /// <see cref="OSVersion"/> 中第 1 节数字
    /// </summary>
    [Required]
    [Comment("OSVersion 中第 1 节数字")]
    [StringLength(5)]
    public required string OSVersionSection1 { get; set; }

    public int OSVersionSection1Number { get; set; }

    /// <summary>
    /// <see cref="OSVersion"/> 中第 2 节数字
    /// </summary>
    [Required]
    [Comment("OSVersion 中第 2 节数字")]
    [StringLength(5)]
    public required string OSVersionSection2 { get; set; }

    public int OSVersionSection2Number { get; set; }

    /// <summary>
    /// <see cref="OSVersion"/> 中第 3 节数字
    /// </summary>
    [Required]
    [Comment("OSVersion 中第 3 节数字")]
    [StringLength(5)]
    public required string OSVersionSection3 { get; set; }

    public int OSVersionSection3Number { get; set; }

    /// <summary>
    /// <see cref="OSVersion"/> 中第 4 节数字
    /// </summary>
    [Required]
    [Comment("OSVersion 中第 4 节数字")]
    [StringLength(5)]
    public required string OSVersionSection4 { get; set; }

    public int OSVersionSection4Number { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    [Comment("数量")]
    public int Count { get; set; }

    /// <summary>
    /// 统计日期（当天的数据）
    /// </summary>
    [Comment("统计日期（当天的数据）")]
    public DateTimeOffset StatisticsTime { get; set; }
}