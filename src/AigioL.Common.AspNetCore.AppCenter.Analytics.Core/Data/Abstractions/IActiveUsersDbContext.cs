using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;


public interface IActiveUsersDbContext : IDbContextBase
{
    DbSet<ActiveUserAnonymousStatistic> ActiveUserRecords { get; }
}
