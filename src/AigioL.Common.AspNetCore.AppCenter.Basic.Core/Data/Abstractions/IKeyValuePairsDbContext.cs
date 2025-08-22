using AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.KeyValuePairs;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using KeyValuePair = global::AigioL.Common.AspNetCore.AppCenter.Basic.Entities.KeyValuePairs.KeyValuePair;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions
{
    public interface IKeyValuePairsDbContext : IDbContextBase
    {
        DbSet<KeyValuePair> KeyValuePairs { get; }

        //DbSet<BinKeyValuePair> BinKeyValuePairs { get; }

        //DbSet<NumberKeyValuePair> NumberKeyValuePairs { get; }
    }
}

#if PROJ_DBCONTEXT_BM
namespace GameTrainer.ApiService.AdminCenter.Data
{
    partial class BMDbContext : IKeyValuePairsDbContext
    {
        public DbSet<KeyValuePair> KeyValuePairs { get; set; } = null!;
    }
}
#endif