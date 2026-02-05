using AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories;

public sealed class ActiveUserRecordRepository<TDbContext>(TDbContext dbContext, IServiceProvider serviceProvider) :
    Repository<TDbContext, ActiveUserAnonymousStatistic, Guid>(dbContext, serviceProvider),
    IActiveUserRecordRepository
    where TDbContext : DbContext, IActiveUsersDbContext
{
    public Task BulkInsertAsync(IEnumerable<ActiveUserAnonymousStatistic> activeUsers)
    {
        return InsertRangeAsync(activeUsers);
    }
}