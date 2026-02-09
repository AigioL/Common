using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Summaries;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;

public interface IOrderSummariesDbContext : IDbContextBase
{
    DbSet<OrderAmountQtySummary> OrderAmountQtySummaries { get; }
}
