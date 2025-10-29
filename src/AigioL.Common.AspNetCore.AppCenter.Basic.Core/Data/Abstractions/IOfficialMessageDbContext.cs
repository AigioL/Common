using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.OfficialMessages;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;

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
