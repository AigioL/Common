using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.KeyValuePairs;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using KeyValuePair = global::AigioL.Common.AspNetCore.AppCenter.Basic.Entities.KeyValuePairs.KeyValuePair;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;

public interface IKeyValuePairsDbContext : IDbContextBase
{
    DbSet<KeyValuePair> KeyValuePairs { get; }

    //DbSet<BinKeyValuePair> BinKeyValuePairs { get; }

    //DbSet<NumberKeyValuePair> NumberKeyValuePairs { get; }
}
