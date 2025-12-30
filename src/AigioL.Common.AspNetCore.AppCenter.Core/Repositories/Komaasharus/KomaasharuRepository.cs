using AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities.Komaasharus;
using AigioL.Common.AspNetCore.AppCenter.Entities.Komaasharus.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Repositories.Komaasharus.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
            CreateTime = DateTimeOffset.Now,
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

partial class KomaasharuRepository<TDbContext>
{
    public async Task<PagedModel<KomaasharuTableItem>> QueryAsync(string? name, KomaasharuType? type, KomaasharuOrientation? orientation, DateTimeOffset?[]? startTime, DateTimeOffset?[]? endTime, bool? expired, bool? disable, string? orderBy, bool? desc, int current, int pageSize, CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.Komaasharus
            .AsNoTrackingWithIdentityResolution();

        if (!string.IsNullOrEmpty(name))
            query = query.Where(a => a.Name.Contains(name));
        if (type.HasValue)
            query = query.Where(a => a.Type == type);
        if (orientation.HasValue)
            query = query.Where(a => a.Orientation == orientation);

        if (startTime != null && startTime.Length == 2)
        {
            if (startTime[0].HasValue)
                query = query.Where(x => x.StartTime >= startTime[0]);
            if (startTime[1].HasValue)
                query = query.Where(x => x.StartTime < startTime[1]);
        }
        if (disable.HasValue)
            query = query.Where(x => x.Disable == disable);
        if (endTime != null && endTime.Length == 2)
        {
            if (endTime[0].HasValue)
                query = query.Where(x => x.EndTime >= endTime[0]);
            if (endTime[1].HasValue)
                query = query.Where(x => x.EndTime < endTime[1]);
        }
        if (expired.HasValue)
        {
            if (expired.Value)
            {
                query = query.Where(x => DateTimeOffset.UtcNow >= x.EndTime);
            }
            else
            {
                query = query.Where(x => DateTimeOffset.UtcNow < x.EndTime);
            }
        }

        if (!string.IsNullOrEmpty(orderBy))
        {
            query = query.OrderByPropertyName(orderBy, desc);
        }
        else
        {
            query = query.OrderBy(x => x.Sort).ThenByDescending(x => x.CreateTime);
        }

        var query2 = query.ProjectTo<KomaasharuTableItem>(mapper.ConfigurationProvider);
        var r = await query2.PagingAsync(current, pageSize, cancellationToken);
        return r;
    }

    public async Task<int> InsertOrUpdateAsync(KomaasharuEdit model)
    {
        (int rowCount, _) = await base.InsertOrUpdateAsync(model);
        return rowCount;
    }

    public async Task<bool> UpdateAsync(
        Guid? operatorUserId,
        KomaasharuEdit model,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var entity = await FindAsync(model.Id, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        mapper.Map(model, entity);
        entity.OperatorUserId = operatorUserId;

        await db.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<bool> InsertAsync(
        Guid? createUserId,
        KomaasharuEdit model,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var entity = mapper.Map<Komaasharu>(model);
        entity.Id = default;
        entity.CreateUserId = createUserId;

        await db.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<KomaasharuEdit?> GetEditByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = db.Komaasharus.AsNoTrackingWithIdentityResolution()
               .Where(x => x.Id == id)
               .Select(FExpressions.MapToEdit);
        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<int> SetDisableAsync(Guid id, bool disable)
    {
        var r = await db.Komaasharus
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Disable, y => disable));
        return r;
    }

    public async Task<StatisticsKomaasharuResponse[]?> GetStatistics(Guid id, CancellationToken cancellationToken = default)
    {
        var query = db.KomaasharuStatistics.AsNoTrackingWithIdentityResolution()
           .Where(x => x.Id == id)
           .OrderBy(x => x.CreateTime);

        var query2 = query.Select(KomaasharuStatistic.Expression);

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

    internal static readonly Expression<Func<Komaasharu, KomaasharuEdit>> MapToEdit = x => new()
    {
        Id = x.Id,
        Name = x.Name,
        Describe = x.Description,
        Url = x.Url,
        JumpUrl = x.JumpUrl,
        StartTime = x.StartTime,
        EndTime = x.EndTime,
        Type = x.Type,
        Orientation = x.Orientation,
        Sort = x.Sort,
        Platform = x.Platform,
        IsAuth = x.IsAuth,
    };
}