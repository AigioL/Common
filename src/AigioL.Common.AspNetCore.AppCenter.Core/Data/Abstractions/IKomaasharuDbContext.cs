using AigioL.Common.AspNetCore.AppCenter.Entities.Komaasharus;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;

public interface IKomaasharuDbContext : IDbContextBase
{
    DbSet<Komaasharu> Komaasharus { get; set; }
}