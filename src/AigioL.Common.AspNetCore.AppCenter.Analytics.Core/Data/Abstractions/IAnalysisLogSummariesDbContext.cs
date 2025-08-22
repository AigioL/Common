using AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs.Summaries;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions
{
    public interface IAnalysisLogSummariesDbContext : IAnalysisLogDbContext
    {
        DbSet<AnalysisEventLogSummary> AnalysisEventLogSummaries { get; }

        DbSet<AnalysisStartServiceLogSummary> AnalysisStartServiceLogSummaries { get; }

        DbSet<AnalysisStartSessionLogSummary> AnalysisStartSessionLogSummaries { get; }

        DbSet<EventRelatedPropertieSummary> EventRelatedPropertieSummaries { get; }
    }
}

#if PROJ_DBCONTEXT_BM
namespace GameTrainer.ApiService.AdminCenter.Data
{
    partial class BMDbContext : IAnalysisLogSummariesDbContext
    {
        public DbSet<AnalysisEventLogSummary> AnalysisEventLogSummaries { get; set; } = null!;

        public DbSet<AnalysisStartServiceLogSummary> AnalysisStartServiceLogSummaries { get; set; } = null!;

        public DbSet<AnalysisStartSessionLogSummary> AnalysisStartSessionLogSummaries { get; set; } = null!;

        public DbSet<EventRelatedPropertieSummary> EventRelatedPropertieSummaries { get; set; } = null!;
    }
}
#endif