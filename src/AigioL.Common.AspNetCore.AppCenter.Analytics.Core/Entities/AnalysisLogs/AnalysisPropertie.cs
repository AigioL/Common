using AigioL.Common.Primitives.Entities.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs;

[Table("AnalysisProperties")]
public partial class AnalysisPropertie :
    Entity<Guid>,
    INEWSEQUENTIALID
{
    public required string Key { get; set; }

    public required string Value { get; set; }

    public virtual List<AnalysisEventLog> EventLogs { get; set; } = null!;

    public virtual List<AnalysisLogPropertiesRelation> Relations { get; set; } = null!;
}
