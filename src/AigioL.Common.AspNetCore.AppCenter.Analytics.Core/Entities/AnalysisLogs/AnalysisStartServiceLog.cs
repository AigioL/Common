using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs;

[Table("AnalysisStartServiceLogs")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class AnalysisStartServiceLog : AnalysisLog
{
    public virtual List<AnalysisService> Services { get; set; } = null!;

    public virtual List<AnalysisServiceLogRelation> ServiceRelations { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<AnalysisStartServiceLog>
    {
        public void Configure(EntityTypeBuilder<AnalysisStartServiceLog> builder)
        {
            builder.HasIndex(x => x.CreationTime);
            builder.HasIndex(x => x.TimeStamp);
            builder.HasIndex(x => x.InstallId);
            builder.HasIndex(x => x.DeviceId);
            builder.HasOne(x => x.App)
               .WithMany(x => x.StartServiceLogs)
               .HasForeignKey(x => x.AppId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Install)
               .WithMany(x => x.StartServiceLogs)
               .HasForeignKey(x => x.InstallId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.AnalysisDevice)
                .WithMany(x => x.StartServiceLogs)
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            //builder.HasMany(x => x.Services)
            //  .WithMany(x => x.StartServices)
            //  .UsingEntity<AnalysisServiceLogRelation>(
            //    j => j.HasOne<AnalysisService>()
            //        .WithMany()
            //        .HasForeignKey(ar => ar.ServiceId),
            //    j => j.HasOne<AnalysisStartServiceLog>()
            //          .WithMany()
            //          .HasForeignKey(ar => ar.StartServiceLogId),
            //    j =>
            //    {
            //        j.HasKey(ar => new { ar.ServiceId, ar.StartServiceLogId });
            //    });

            builder.HasMany(x => x.Services)
                   .WithMany(x => x.StartServices)
                   .UsingEntity<AnalysisServiceLogRelation>(
                     j => j.HasOne(x => x.Service)
                           .WithMany(x => x.ServiceLogRelationss)
                           .HasForeignKey(x => x.ServiceId)
                           .OnDelete(DeleteBehavior.Cascade),
                     j => j.HasOne(x => x.StartServiceLog)
                           .WithMany(x => x.ServiceRelations)
                           .HasForeignKey(x => x.StartServiceLogId)
                           .OnDelete(DeleteBehavior.Cascade),
                     j =>
                     {
                         j.HasKey(ar => new { ar.StartServiceLogId, ar.ServiceId });
                     });
        }
    }
}
