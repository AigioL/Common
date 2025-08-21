using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs;

[Table("AnalysisStartSessionLogs")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class AnalysisStartSessionLog : AnalysisLog
{
    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<AnalysisStartSessionLog>
    {
        public void Configure(EntityTypeBuilder<AnalysisStartSessionLog> builder)
        {
            builder.HasIndex(x => x.CreationTime);
            builder.HasIndex(x => x.TimeStamp);
            builder.HasIndex(x => x.InstallId);
            builder.HasIndex(x => x.DeviceId);
            builder.HasOne(x => x.App)
               .WithMany(x => x.StartSessionLogs)
               .HasForeignKey(x => x.AppId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Install)
               .WithMany(x => x.StartSessionLogs)
               .HasForeignKey(x => x.InstallId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.AnalysisDevice)
               .WithMany(x => x.StartSessionLogs)
               .HasForeignKey(x => x.DeviceId)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}