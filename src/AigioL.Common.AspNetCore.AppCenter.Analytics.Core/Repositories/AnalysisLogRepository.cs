using AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories;

sealed partial class AnalysisLogRepository<TDbContext>(TDbContext dbContext, IServiceProvider serviceProvider) :
    Repository<TDbContext, AnalysisEventLog, Guid>(dbContext, serviceProvider),
    IAnalysisLogRepository
    where TDbContext : DbContext, IAnalysisLogDbContext
{
}

partial class AnalysisLogRepository<TDbContext>
{
    public async Task<IdNameModel[]?> GetAppIdListAsync(CancellationToken cancellationToken = default)
    {
        var query = db.AnalysisApps
            .AsNoTrackingWithIdentityResolution()
            .Select(x => new IdNameModel
            {
                Id = x.Id,
                Name = x.Name,
            });

        var r = await query.ToArrayAsync(cancellationToken);
        return r;
    }
}