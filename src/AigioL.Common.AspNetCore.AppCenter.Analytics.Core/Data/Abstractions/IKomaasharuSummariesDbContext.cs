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

#if !REMOVE_APP_DBCONTEXT
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.Data
{
    partial class AppDbContext : IKomaasharuSummariesDbContext
    {
        public DbSet<KomaasharuStatistic> KomaasharuStatistics { get; set; } = null!;

        public DbSet<KomaasharuStatisticPerDaySummary> KomaasharuStatisticPerDaySummaries { get; set; } = null!;
    }
}
#endif