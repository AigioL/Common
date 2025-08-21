using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs;

[Table("AnalysisInstalls")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class AnalysisInstall :
    Entity<Guid>,
    INEWSEQUENTIALID,
    ICreationTime
{
    [Required]
    public required string InstallId { get; set; }

    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }

    public virtual List<AnalysisEventLog> EventLogs { get; set; } = null!;

    public virtual List<AnalysisStartServiceLog> StartServiceLogs { get; set; } = null!;

    public virtual List<AnalysisStartSessionLog> StartSessionLogs { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<AnalysisInstall>
    {
        public void Configure(EntityTypeBuilder<AnalysisInstall> builder)
        {
            builder.HasIndex(x => x.InstallId);
            builder.HasAlternateKey(x => x.InstallId);
        }
    }
}
