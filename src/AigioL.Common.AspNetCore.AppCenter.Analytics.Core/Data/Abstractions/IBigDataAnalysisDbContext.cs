#if DEBUG
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;

[Obsolete]
public interface IBigDataAnalysisDbContext : IDbContextBase
{
    //DbSet<AuthMessageRecord> AuthMessageRecords { get; set; }

    //DbSet<EmailSendRecord> EmailSendRecords { get; set; }

    //DbSet<OrderAmountQtySummary> OrderAmountQtySummaries { get; set; }
}
#endif