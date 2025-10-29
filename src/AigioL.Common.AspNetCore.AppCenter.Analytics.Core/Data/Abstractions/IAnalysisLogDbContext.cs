using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;

public interface IAnalysisLogDbContext : IDbContextBase
{
    DbSet<AnalysisPropertie> AnalysisProperties { get; }

    DbSet<AnalysisApp> AnalysisApps { get; }

    DbSet<AnalysisInstall> AnalysisInstalls { get; }

    DbSet<AnalysisEventLog> AnalysisEventLogs { get; }

    DbSet<AnalysisStartServiceLog> AnalysisStartServiceLogs { get; }

    DbSet<AnalysisStartSessionLog> AnalysisStartSessionLogs { get; }

    DbSet<AnalysisDevice> AnalysisDevices { get; }

    DbSet<AnalysisService> AnalysisServices { get; }

    DbSet<AnalysisLogPropertiesRelation> AnalysisLogPropertiesRelations { get; }

    DbSet<AnalysisServiceLogRelation> AnalysisServiceLogRelations { get; }
}