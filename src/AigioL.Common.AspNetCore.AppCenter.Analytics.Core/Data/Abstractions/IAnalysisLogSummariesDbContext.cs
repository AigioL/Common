using AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs.Summaries;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;

public interface IAnalysisLogSummariesDbContext : IAnalysisLogDbContext
{
    DbSet<AnalysisEventLogSummary> AnalysisEventLogSummaries { get; }

    DbSet<AnalysisStartServiceLogSummary> AnalysisStartServiceLogSummaries { get; }

    DbSet<AnalysisStartSessionLogSummary> AnalysisStartSessionLogSummaries { get; }

    DbSet<EventRelatedPropertieSummary> EventRelatedPropertieSummaries { get; }
}