using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs;

[Table("AnalysisServiceLogRelations")]
public partial class AnalysisServiceLogRelation
{
    public Guid ServiceId { get; set; }

    public Guid StartServiceLogId { get; set; }

    public virtual AnalysisService Service { get; set; } = null!;

    public virtual AnalysisStartServiceLog StartServiceLog { get; set; } = null!;
}