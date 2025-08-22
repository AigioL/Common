#if DEBUG
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;

[Obsolete("use IActiveUsersDbContext OR IKomaasharuDbContext OR IAnalysisLogDbContext OR IXXXXSummariesDbContext", true)]
public interface IBigDataAnalysisDbContext : IDbContextBase
{
    //DbSet<AuthMessageRecord> AuthMessageRecords { get; }

    //DbSet<EmailSendRecord> EmailSendRecords { get; }

    //DbSet<OrderAmountQtySummary> OrderAmountQtySummaries { get; }
}
#endif