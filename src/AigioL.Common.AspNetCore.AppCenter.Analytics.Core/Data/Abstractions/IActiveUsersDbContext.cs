using AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs.Summaries;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions
{

    public interface IActiveUsersDbContext : IDbContextBase
    {
        DbSet<ActiveUserAnonymousStatistic> ActiveUserRecords { get; }
    }
}

#if PROJ_DBCONTEXT_BM // TODO: 使用源生成器标注 dbcontext 子类检查接口自动补全 DbSet 属性后，删除这些 #if PROJ_DBCONTEXT_BM 块代码
namespace GameTrainer.ApiService.AdminCenter.Data
{
    partial class BMDbContext : IActiveUsersDbContext
    {
        public DbSet<ActiveUserAnonymousStatistic> ActiveUserRecords { get; set; } = null!;
    }
}
#endif
