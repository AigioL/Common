using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs;

[Table("AnalysisLogPropertiesRelations")]
public partial class AnalysisLogPropertiesRelation
{
    public Guid EventLogId { get; set; }

    public Guid PropertiesId { get; set; }

    public virtual AnalysisEventLog EventLog { get; set; } = null!;

    public virtual AnalysisPropertie Properties { get; set; } = null!;
}
