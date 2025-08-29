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

#if !REMOVE_APP_DBCONTEXT
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.Data
{
    partial class AppDbContext : IAnalysisLogSummariesDbContext
    {
        public DbSet<AnalysisEventLogSummary> AnalysisEventLogSummaries { get; set; } = null!;

        public DbSet<AnalysisStartServiceLogSummary> AnalysisStartServiceLogSummaries { get; set; } = null!;

        public DbSet<AnalysisStartSessionLogSummary> AnalysisStartSessionLogSummaries { get; set; } = null!;

        public DbSet<EventRelatedPropertieSummary> EventRelatedPropertieSummaries { get; set; } = null!;
    }
}
#endif