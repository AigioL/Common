using AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities.Komaasharus;
using AigioL.Common.AspNetCore.AppCenter.Entities.Komaasharus.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus;
using AigioL.Common.AspNetCore.AppCenter.Repositories.Komaasharus.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AppCenter.Repositories.Komaasharus;

sealed partial class KomaasharuRepository<TDbContext> :
    Repository<TDbContext, Komaasharu, Guid>,
    IKomaasharuRepository
    where TDbContext : DbContext, IKomaasharuDbContext, IKomaasharuSummariesDbContext
{
    public KomaasharuRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }
}

partial class KomaasharuRepository<TDbContext>
{
    public async Task AddCounterAsync(Guid id, long count, long clickCount)
    {
        await db.KomaasharuStatistics.AddAsync(new KomaasharuStatistic
        {
            KomaasharuId = id,
            NumDisplay = count,
            NumClick = clickCount,
            CreationTime = DateTimeOffset.Now,
        });
        await db.Komaasharus.Where(x => x.Id == id).ExecuteUpdateAsync(setters =>
        {
            setters.SetProperty(b => b.TotalDisplay, b => b.TotalDisplay + count);
            setters.SetProperty(b => b.TotalClick, b => b.TotalClick + clickCount);
        });
        await db.SaveChangesAsync();
    }

    public async Task<Komaasharu[]> GetAllEntitiesAsync()
    {
        var query = db.Komaasharus.AsNoTrackingWithIdentityResolution()
            .Where(FExpressions.ValidityPeriod);
#if DEBUG
        var str = query.ToQueryString();
        //Console.WriteLine(str);
#endif
        var r = await query.ToArrayAsync(RequestAborted);
        return r;
    }

    public async Task<KomaasharuRedisModel[]> GetCacheModelsAsync()
    {
        var query = db.Komaasharus.AsNoTrackingWithIdentityResolution()
            .Where(FExpressions.ValidityPeriod);
        var query2 = query.Select(FExpressions.MapToRedisModel);
#if DEBUG
        var str = query2.ToQueryString();
        //Console.WriteLine(str);
#endif
        var r = await query2.ToArrayAsync(RequestAborted);
        return r;
    }

    public async Task<KomaasharuModel[]> GetAllAsync(
        KomaasharuType? type = null,
        DevicePlatform2? platform = null,
        DeviceIdiom? deviceIdiom = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.Komaasharus.AsNoTrackingWithIdentityResolution()
            .Where(FExpressions.ValidityPeriod);
        if (type.HasValue)
        {
            query = query.Where(x => x.Type == type.Value);
        }
        if (platform.HasValue)
        {
            var platform2 = platform.Value.ToWebApiCompat();
            query = query.Where(x => x.Platform.HasFlag(platform2));
        }
        if (deviceIdiom.HasValue)
        {
            query = query.Where(x => x.DeviceIdiom.HasFlag(deviceIdiom.Value));
        }
        var query2 = query.Select(FExpressions.MapToModel);
#if DEBUG
        var str = query2.ToQueryString();
        //Console.WriteLine(str);
#endif
        var r = await query2.ToArrayAsync(cancellationToken);
        return r;
    }
}

file static class FExpressions
{
    internal static readonly Expression<Func<Komaasharu, bool>> ValidityPeriod = x =>
        !x.Disable && x.StartTime <= DateTimeOffset.Now && DateTimeOffset.Now <= x.EndTime;

    internal static readonly Expression<Func<Komaasharu, KomaasharuModel>> MapToModel = x => new KomaasharuModel()
    {
        Id = x.Id,
        Name = x.Name,
        Desc = x.Description,
        Orientation = x.Orientation,
        Type = x.Type,
        Sort = x.Sort,
    };

    internal static readonly Expression<Func<Komaasharu, KomaasharuRedisModel>> MapToRedisModel = x => new()
    {
        Id = x.Id,
        Name = x.Name,
        Desc = x.Description,
        Orientation = x.Orientation,
        Type = x.Type,
        Sort = x.Sort,
        ImageUrl = x.Url,
        JumpUrl = x.JumpUrl,
        DeviceIdiom = x.DeviceIdiom,
        Platform = x.Platform,
        IsAuth = x.IsAuth,
    };
}