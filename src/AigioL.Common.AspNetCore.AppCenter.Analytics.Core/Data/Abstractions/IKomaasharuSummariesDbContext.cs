using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities.Komaasharus.Summaries;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions
{
    public interface IKomaasharuSummariesDbContext : IKomaasharuDbContext
    {
        DbSet<KomaasharuStatistic> KomaasharuStatistics { get; }

        DbSet<KomaasharuStatisticPerDaySummary> KomaasharuStatisticPerDaySummaries { get; }
    }
}