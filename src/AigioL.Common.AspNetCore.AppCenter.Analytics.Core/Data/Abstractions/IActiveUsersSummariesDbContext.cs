using AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs.Summaries;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions
{
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
}

#if PROJ_DBCONTEXT_BM
namespace GameTrainer.ApiService.AdminCenter.Data
{
    partial class BMDbContext : IActiveUsersSummariesDbContext
    {
        public DbSet<ActiveUserScreenResolutionSummary> ActiveUserScreenResolutionSummaries { get; set; } = null!;

        public DbSet<ActiveUserStatisticSummary> ActiveUserStatisticSummaries { get; set; } = null!;

        public DbSet<ActiveUserPlatformSummary> ActiveUserPlatformSummaries { get; set; } = null!;

        public DbSet<ActiveUserOSSummary> ActiveUserOSSummaries { get; set; } = null!;

        public DbSet<ActiveUserArchitectureSummary> ActiveUserArchitectureSummaries { get; set; } = null!;

        public DbSet<ActiveUserDayWeekMonthSummary> ActiveUserDayWeekMonthSummaries { get; set; } = null!;

        public DbSet<ActiveUserDeviceSummary> ActiveUserDeviceSummaries { get; set; } = null!;

        public DbSet<ActiveUserLanguageSummary> ActiveUserLanguageSummaries { get; set; } = null!;

        public DbSet<ActiveUserAppVerSummary> ActiveUserAppVerSummaries { get; set; } = null!;
    }
}
#endif