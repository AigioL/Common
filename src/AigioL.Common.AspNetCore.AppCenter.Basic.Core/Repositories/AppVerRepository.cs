using AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories;

public sealed partial class AppVerRepository<TDbContext> :
    Repository<TDbContext, AppVer, Guid>,
    IAppVerRepository
    where TDbContext : DbContext, IAppVerDbContext
{
    public AppVerRepository(
        TDbContext dbContext,
        IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    /// <inheritdoc/>
    public async Task<AppVer[]> GetAppVerAllAsync()
    {
        var r = await Entity.AsNoTrackingWithIdentityResolution().Where(x => !x.Disable).Take(200).ToArrayAsync();
        return r;
    }
}

partial class AppVerRepository<TDbContext> // 管理后台
{
    public async Task<PagedModel<AppVersionTableItemModel>> QueryAsync(
        string? version,
        bool? disable,
        bool? beta = false,
        string? orderBy = null,
        bool? desc = null,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.AppVers
            .AsNoTrackingWithIdentityResolution();
        if (!string.IsNullOrWhiteSpace(version))
            query = query.Where(x => x.Version != null && x.Version.Contains(version));
        if (disable.HasValue)
            query = query.Where(x => x.Disable == disable);
        if (beta.HasValue)
            query = query.Where(x => x.BetaVersion == beta);
        if (!string.IsNullOrEmpty(orderBy))
        {
            query = query.OrderByPropertyName(orderBy, desc);
        }
        else
        {
            query = query.OrderByDescending(x => x.CreateTime);
        }
        var r = await query
            .Include(x => x.Builds)
            .ProjectTo<AppVersionTableItemModel>(mapper.ConfigurationProvider)
            .PagingAsync(current, pageSize, RequestAborted);
        return r;
    }

    public async Task<AddOrEditAppVersionModel?> GetEditByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.AppVers
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == id)
            .ProjectTo<AddOrEditAppVersionModel>(mapper.ConfigurationProvider);
        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<bool> UpdateAsync(
        Guid? operatorUserId,
        AddOrEditAppVersionModel model,
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
        AddOrEditAppVersionModel model,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var entity = mapper.Map<AppVer>(model);
        entity.Id = default;
        entity.CreateUserId = createUserId;

        await db.SaveChangesAsync(CancellationToken.None);
        return true;
    }
}