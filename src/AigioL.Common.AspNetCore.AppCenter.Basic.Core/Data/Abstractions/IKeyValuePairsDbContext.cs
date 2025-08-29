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

#if !REMOVE_APP_DBCONTEXT
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.Data
{
    partial class AppDbContext : IKeyValuePairsDbContext
    {
        public DbSet<KeyValuePair> KeyValuePairs { get; set; } = null!;
    }
}
#endif