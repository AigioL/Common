using AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.Komaasharu;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions
{
    public interface IKomaasharuDbContext : IDbContextBase
    {
        DbSet<Komaasharu> Komaasharus { get; set; }
    }
}

#if PROJ_DBCONTEXT_BM
namespace GameTrainer.ApiService.AdminCenter.Data
{
    partial class BMDbContext : IKomaasharuDbContext
    {
        public DbSet<Komaasharu> Komaasharus { get; set; } = null!;
    }
}
#endif
