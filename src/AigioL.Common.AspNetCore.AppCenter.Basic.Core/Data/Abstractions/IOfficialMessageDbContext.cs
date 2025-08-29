using AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.OfficialMessages;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions
{
    public interface IOfficialMessageDbContext : IDbContextBase
    {
        DbSet<OfficialMessage> OfficialMessages { get; }

        DbSet<OfficialMessageAppVerRelation> OfficialMessageAppVerRelations { get; }
    }

#if DEBUG
    [Obsolete("use IOfficialMessageDbContext", true)]
    public interface IBasicServicesDbContext : IOfficialMessageDbContext
    {
    }
#endif
}

#if !REMOVE_APP_DBCONTEXT
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.Data
{
    partial class AppDbContext : IOfficialMessageDbContext
    {
        public DbSet<OfficialMessage> OfficialMessages { get; set; } = null!;

        public DbSet<OfficialMessageAppVerRelation> OfficialMessageAppVerRelations { get; set; } = null!;
    }
}
#endif