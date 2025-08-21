using AigioL.Common.Primitives.Entities.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs;

[Table("AnalysisServices")]
public partial class AnalysisService :
    Entity<Guid>,
    INEWSEQUENTIALID
{
    public required string Name { get; set; }

    public virtual List<AnalysisStartServiceLog> StartServices { get; set; } = null!;

    public virtual List<AnalysisServiceLogRelation> ServiceLogRelationss { get; set; } = null!;
}
