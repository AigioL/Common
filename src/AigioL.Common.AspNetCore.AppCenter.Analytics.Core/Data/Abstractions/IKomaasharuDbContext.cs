using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.Komaasharu;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;

public interface IKomaasharuDbContext : IDbContextBase
{
    DbSet<Komaasharu> Komaasharus { get; set; }
}
