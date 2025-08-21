using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs.Summaries;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;

public interface IAnalysisLogSummariesDbContext : IAnalysisLogDbContext
{
    DbSet<AnalysisEventLogSummary> AnalysisEventLogSummaries { get; set; }

    DbSet<AnalysisStartServiceLogSummary> AnalysisStartServiceLogSummaries { get; set; }

    DbSet<AnalysisStartSessionLogSummary> AnalysisStartSessionLogSummaries { get; set; }

    DbSet<EventRelatedPropertieSummary> EventRelatedPropertieSummaries { get; set; }
}
