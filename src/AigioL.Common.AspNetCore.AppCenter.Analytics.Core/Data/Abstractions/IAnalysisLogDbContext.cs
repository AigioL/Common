using AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions
{
    public interface IAnalysisLogDbContext : IDbContextBase
    {
        DbSet<AnalysisPropertie> AnalysisProperties { get; }

        DbSet<AnalysisApp> AnalysisApps { get; }

        DbSet<AnalysisInstall> AnalysisInstalls { get; }

        DbSet<AnalysisEventLog> AnalysisEventLogs { get; }

        DbSet<AnalysisStartServiceLog> AnalysisStartServiceLogs { get; }

        DbSet<AnalysisStartSessionLog> AnalysisStartSessionLogs { get; }

        DbSet<AnalysisDevice> AnalysisDevices { get; }

        DbSet<AnalysisService> AnalysisServices { get; }

        DbSet<AnalysisLogPropertiesRelation> AnalysisLogPropertiesRelations { get; }

        DbSet<AnalysisServiceLogRelation> AnalysisServiceLogRelations { get; }
    }
}

#if !REMOVE_APP_DBCONTEXT
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.Data
{
    partial class AppDbContext : IAnalysisLogDbContext
    {
        public DbSet<AnalysisPropertie> AnalysisProperties { get; set; } = null!;

        public DbSet<AnalysisApp> AnalysisApps { get; set; } = null!;

        public DbSet<AnalysisInstall> AnalysisInstalls { get; set; } = null!;

        public DbSet<AnalysisEventLog> AnalysisEventLogs { get; set; } = null!;

        public DbSet<AnalysisStartServiceLog> AnalysisStartServiceLogs { get; set; } = null!;

        public DbSet<AnalysisStartSessionLog> AnalysisStartSessionLogs { get; set; } = null!;

        public DbSet<AnalysisDevice> AnalysisDevices { get; set; } = null!;

        public DbSet<AnalysisService> AnalysisServices { get; set; } = null!;

        public DbSet<AnalysisLogPropertiesRelation> AnalysisLogPropertiesRelations { get; set; } = null!;

        public DbSet<AnalysisServiceLogRelation> AnalysisServiceLogRelations { get; set; } = null!;
    }
}
#endif