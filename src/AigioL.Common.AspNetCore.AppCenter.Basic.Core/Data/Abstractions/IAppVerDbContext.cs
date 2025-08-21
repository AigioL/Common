using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;

public interface IAppVerDbContext : IDbContextBase
{
    DbSet<AppVer> AppVers { get; set; }

    DbSet<AppVerBuild> AppVerBuilds { get; set; }

    DbSet<AppVerFile> AppVerFiles { get; set; }

    //DbSet<AppCloudConfig> AppCloudConfigs { get; set; }

    //DbSet<InstallerVer> InstallerVers { get; set; }

    //DbSet<InstallerVerBuild> InstallerVerBuilds { get; set; }

    //DbSet<InstallerVerFile> InstallerVerFiles { get; set; }
}
