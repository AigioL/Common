using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.Komaasharu.Summaries;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;

public interface IKomaasharuSummariesDbContext : IKomaasharuDbContext
{
    DbSet<KomaasharuStatistic> KomaasharuStatistics { get; set; }

    DbSet<KomaasharuStatisticPerDaySummary> KomaasharuStatisticPerDaySummaries { get; set; }
}
