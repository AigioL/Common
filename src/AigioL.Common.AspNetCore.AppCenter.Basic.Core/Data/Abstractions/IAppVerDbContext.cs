using AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions
{
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
}

#if !REMOVE_APP_DBCONTEXT
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.Data
{
    partial class AppDbContext : IAppVerDbContext
    {
        public DbSet<AppVer> AppVers { get; set; } = null!;

        public DbSet<AppVerBuild> AppVerBuilds { get; set; } = null!;

        public DbSet<AppVerFile> AppVerFiles { get; set; } = null!;
    }
}
#endif