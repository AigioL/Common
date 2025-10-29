using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs.Summaries;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;

public interface IActiveUsersSummariesDbContext : IActiveUsersDbContext
{
    DbSet<ActiveUserScreenResolutionSummary> ActiveUserScreenResolutionSummaries { get; }

    DbSet<ActiveUserStatisticSummary> ActiveUserStatisticSummaries { get; }

    DbSet<ActiveUserPlatformSummary> ActiveUserPlatformSummaries { get; }

    DbSet<ActiveUserOSSummary> ActiveUserOSSummaries { get; }

    DbSet<ActiveUserArchitectureSummary> ActiveUserArchitectureSummaries { get; }

    DbSet<ActiveUserDayWeekMonthSummary> ActiveUserDayWeekMonthSummaries { get; }

    DbSet<ActiveUserDeviceSummary> ActiveUserDeviceSummaries { get; }

    DbSet<ActiveUserLanguageSummary> ActiveUserLanguageSummaries { get; }

    DbSet<ActiveUserAppVerSummary> ActiveUserAppVerSummaries { get; }
}