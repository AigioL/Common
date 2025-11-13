using AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories;

public sealed class AppVerRepository<TDbContext> :
    Repository<TDbContext, AppVer, Guid>,
    IAppVerRepository
    where TDbContext : DbContext, IAppVerDbContext
{
    public AppVerRepository(
        TDbContext dbContext,
        IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    /// <inheritdoc/>
    public async Task<AppVer[]> GetAppVerAllAsync()
    {
        var r = await Entity.AsNoTrackingWithIdentityResolution().Where(x => !x.Disable).Take(200).ToArrayAsync();
        return r;
    }
}
