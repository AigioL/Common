using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;

public interface IAppVerDbContext : IDbContextBase
{
    DbSet<AppVer> AppVers { get; }

    DbSet<AppVerBuild> AppVerBuilds { get; }

    DbSet<AppVerFile> AppVerFiles { get; }

    //DbSet<AppCloudConfig> AppCloudConfigs { get; }

    //DbSet<InstallerVer> InstallerVers { get; }

    //DbSet<InstallerVerBuild> InstallerVerBuilds { get; }

    //DbSet<InstallerVerFile> InstallerVerFiles { get; }
}