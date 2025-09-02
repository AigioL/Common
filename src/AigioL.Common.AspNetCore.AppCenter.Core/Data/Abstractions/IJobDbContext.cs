using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;

public interface IJobDbContext : IDbContextBase
{
    DbSet<JobRecordResult> JobRecordResults { get; }
}
