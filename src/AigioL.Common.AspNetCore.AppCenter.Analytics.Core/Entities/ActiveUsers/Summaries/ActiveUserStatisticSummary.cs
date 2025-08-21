using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers.Summaries;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers.Summaries;

/// <summary>
/// 活跃用户统计汇总
/// </summary>
[Table("ActiveUserStatisticSummaries")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class ActiveUserStatisticSummary :
    Entity<Guid>,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 总数据量
    /// </summary>
    public int AllCount { get; set; }

    /// <summary>
    /// 登录数量
    /// </summary>
    public int LoginCount { get; set; }

    /// <summary>
    /// 带设备 Id 数量
    /// </summary>
    public int DeviceIdCount { get; set; }

    /// <summary>
    /// IP 去重后数量
    /// </summary>
    public int IPCount { get; set; }

    /// <summary>
    /// 统计日期（开始）
    /// </summary>
    public DateTimeOffset StatisticsStartTime { get; set; }

    /// <summary>
    /// 统计日期（结束）
    /// </summary>
    public DateTimeOffset StatisticsEndTime { get; set; }

    /// <summary>
    /// 统计平台数据
    /// </summary>
    public virtual List<ActiveUserPlatformSummary> Platforms { get; set; } = null!;

    /// <summary>
    /// 统计显示器数据
    /// </summary>
    public virtual List<ActiveUserScreenResolutionSummary> Screens { get; set; } = null!;

    /// <summary>
    /// 统计系统版本数据
    /// </summary>
    public virtual List<ActiveUserOSSummary> OSVersions { get; set; } = null!;

    /// <summary>
    /// 统计 CPU 架构(处理器体系结构) 数据
    /// </summary>
    public virtual List<ActiveUserArchitectureSummary> Architectures { get; set; } = null!;

    public static Expression<Func<ActiveUserStatisticSummary, ActiveUserSumResponse>> Expression => _.Expression;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<ActiveUserStatisticSummary>
    {
        public void Configure(EntityTypeBuilder<ActiveUserStatisticSummary> builder)
        {
            builder.HasMany(x => x.OSVersions)
                .WithOne()
                .HasForeignKey(x => x.StatisticsId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.Platforms)
                .WithOne()
                .HasForeignKey(x => x.StatisticsId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.Platforms)
                .WithOne()
                .HasForeignKey(x => x.StatisticsId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.Screens)
                .WithOne()
                .HasForeignKey(x => x.StatisticsId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.Architectures)
                .WithOne()
                .HasForeignKey(x => x.StatisticsId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

file static class _
{
    internal static readonly Expression<Func<ActiveUserStatisticSummary, ActiveUserSumResponse>> Expression = x => new()
    {
        AllCount = x.AllCount,
        LoginCount = x.LoginCount,
        DeviceIdCount = x.DeviceIdCount,
        IPCount = x.IPCount,
        Time = x.StatisticsStartTime.Date,
    };
}