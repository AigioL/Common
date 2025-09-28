using AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories;

sealed partial class AppVerBuildRepository<TDbContext> :
    Repository<TDbContext, AppVerBuild, Guid>,
    IAppVerBuildRepository
    where TDbContext : DbContext, IAppVerDbContext
{
    public AppVerBuildRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }
}

partial class AppVerBuildRepository<TDbContext>
{
    public async Task<AppVersionModel?> GetLatestVersionAsync(
        IReadOnlyAppVer? appVer,
        ClientPlatform platform,
        Version? osVersion,
        DeploymentMode deploymentMode,
        bool includeBeta)
    {
        throw new NotSupportedException();
    }
}