using AigioL.Common.Primitives.Entities.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs;

[Table("AnalysisApps")]
public partial class AnalysisApp :
    Entity<Guid>,
    INEWSEQUENTIALID
{
    [Required]
    public required string Name { get; set; }

    public string? Description { get; set; }

    public virtual List<AnalysisEventLog> EventLogs { get; set; } = null!;

    public virtual List<AnalysisStartServiceLog> StartServiceLogs { get; set; } = null!;

    public virtual List<AnalysisStartSessionLog> StartSessionLogs { get; set; } = null!;
}
