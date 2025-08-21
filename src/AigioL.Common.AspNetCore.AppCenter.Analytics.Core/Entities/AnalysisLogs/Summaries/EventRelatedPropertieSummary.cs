using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs.Summaries;

/// <summary>
/// 事件关联属性统计表实体类
/// </summary>
[Table("EventRelatedPropertieSummaries")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class EventRelatedPropertieSummary :
    Entity<Guid>,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 事件统计 Id
    /// </summary>
    [Comment("事件统计 Id")]
    public Guid EventSummaryId { get; set; }

    /// <summary>
    /// 属性值
    /// </summary>
    [Required]
    [Comment("属性值")]
    public required string PropertieValue { get; set; }

    /// <summary>
    /// 属性键
    /// </summary>
    [Required]
    [Comment("属性键")]
    public required string PropertieKey { get; set; }

    /// <summary>
    /// 统计值
    /// </summary>
    [Comment("统计值")]
    public int StatisticalValues { get; set; }

    public virtual AnalysisEventLogSummary EventSummary { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<EventRelatedPropertieSummary>
    {
        public void Configure(EntityTypeBuilder<EventRelatedPropertieSummary> builder)
        {
            builder
                .HasOne(x => x.EventSummary)
                .WithMany(g => g.EventRelatedPropertie)
                .HasForeignKey(x => x.EventSummaryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
