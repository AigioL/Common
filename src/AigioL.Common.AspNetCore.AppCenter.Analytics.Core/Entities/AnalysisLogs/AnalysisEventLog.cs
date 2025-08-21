using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs;

/// <summary>
/// 统计日志（点击日志等）表实体类
/// </summary>
[Table("AnalysisEventLogs")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class AnalysisEventLog : AnalysisLog
{
    [Required]
    public required string Name { get; set; }

    public virtual List<AnalysisPropertie> Properties { get; set; } = null!;

    public virtual List<AnalysisLogPropertiesRelation> PropertiesRelations { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<AnalysisEventLog>
    {
        public void Configure(EntityTypeBuilder<AnalysisEventLog> builder)
        {
            builder.HasIndex(x => x.CreationTime);
            builder.HasIndex(x => x.TimeStamp);
            builder.HasIndex(x => x.InstallId);
            builder.HasIndex(x => x.DeviceId);
            builder.HasOne(x => x.App)
               .WithMany(x => x.EventLogs)
               .HasForeignKey(x => x.AppId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Install)
               .WithMany(x => x.EventLogs)
               .HasForeignKey(x => x.InstallId)
               .OnDelete(DeleteBehavior.Cascade);

            //builder.HasMany(x => x.Properties)
            //    .WithMany(x => x.EventLogs)
            //    .UsingEntity<AnalysisLogPropertiesRelation>(
            //      j => j.HasOne<AnalysisPropertie>()
            //          .WithMany()
            //          .HasForeignKey(ar => ar.PropertiesId),
            //      j => j.HasOne<AnalysisEventLog>()
            //            .WithMany()
            //            .HasForeignKey(ar => ar.EventLogId),
            //      j =>
            //      {
            //          j.HasKey(ar => new { ar.EventLogId, ar.PropertiesId });
            //      });

            builder.HasOne(x => x.AnalysisDevice)
                  .WithMany(x => x.EventLogs)
                  .HasForeignKey(x => x.DeviceId)
                  .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Properties)
                   .WithMany(x => x.EventLogs)
                   .UsingEntity<AnalysisLogPropertiesRelation>(
                     j => j.HasOne(x => x.Properties)
                           .WithMany(x => x.Relations)
                           .HasForeignKey(x => x.PropertiesId)
                           .OnDelete(DeleteBehavior.Cascade),
                     j => j.HasOne(x => x.EventLog)
                           .WithMany(x => x.PropertiesRelations)
                           .HasForeignKey(x => x.EventLogId)
                           .OnDelete(DeleteBehavior.Cascade),
                     j =>
                     {
                         j.HasKey(ar => new { ar.PropertiesId, ar.EventLogId });
                     });
        }
    }
}