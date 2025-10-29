using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using KeyValuePair = global::AigioL.Common.AspNetCore.AppCenter.Entities.KeyValuePair;

namespace AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;

public interface IKeyValuePairsDbContext : IDbContextBase
{
    DbSet<KeyValuePair> KeyValuePairs { get; }
}