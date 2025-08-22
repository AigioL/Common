using AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.Komaasharu.Summaries;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions
{
    public interface IKomaasharuSummariesDbContext : IKomaasharuDbContext
    {
        DbSet<KomaasharuStatistic> KomaasharuStatistics { get; }

        DbSet<KomaasharuStatisticPerDaySummary> KomaasharuStatisticPerDaySummaries { get; }
    }
}

#if PROJ_DBCONTEXT_BM
namespace GameTrainer.ApiService.AdminCenter.Data
{
    partial class BMDbContext : IKomaasharuSummariesDbContext
    {
        public DbSet<KomaasharuStatistic> KomaasharuStatistics { get; set; } = null!;

        public DbSet<KomaasharuStatisticPerDaySummary> KomaasharuStatisticPerDaySummaries { get; set; } = null!;
    }
}
#endif