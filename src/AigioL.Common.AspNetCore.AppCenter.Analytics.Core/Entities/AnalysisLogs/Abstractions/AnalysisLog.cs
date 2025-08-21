using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs.Abstractions;

public abstract partial class AnalysisLog :
    Entity<Guid>,
    INEWSEQUENTIALID,
    ICreationTime
{
    public Guid? DeviceId { get; set; }

    public virtual AnalysisDevice AnalysisDevice { get; set; } = null!;

    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }

    public required Guid AppId { get; set; }

    public required Guid InstallId { get; set; }

    public virtual AnalysisInstall Install { get; set; } = null!;

    public virtual AnalysisApp App { get; set; } = null!;

    public DateTimeOffset? TimeStamp { get; set; }
}
