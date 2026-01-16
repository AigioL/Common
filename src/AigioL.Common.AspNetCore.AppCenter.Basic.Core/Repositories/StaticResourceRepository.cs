using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.FileSystem;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.FileSystem;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Storage;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories;

sealed partial class StaticResourceRepository<TDbContext> :
    Repository<TDbContext, StaticResource, Guid>,
    IStaticResourceRepository where TDbContext : DbContext
{
    public StaticResourceRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    public async Task<(string? filePath, CloudFileType fileType)> GetFilePathBySha384WithFileExtAsync(
        string sha384,
        string fileExt,
        CancellationToken cancellationToken = default)
    {
        var query = EntityNoTracking
            .Where(x => x.SHA384 == sha384 && x.FileExtension == fileExt)
            .Select(static x => new
            {
                x.FilePath,
                x.FileType,
            });
        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r == null ? default : (r.FilePath, r.FileType);
    }
}

partial class StaticResourceRepository<TDbContext> // 管理后台
{
    public async Task<PagedModel<StaticResourceTableItemModel>> QueryAsync(
        string? fileName,
        string? filePath,
        CloudFileType? fileType,
        string? sha384,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();

        const int takeIncludeCount = 200;

        var query = EntityNoTracking
            .Include(x => x.StaticResourceUploadRecords!.Take(takeIncludeCount))
            .Where(x => x.DeleteTime == null);
        if (!string.IsNullOrEmpty(fileName))
            query = query.Where(x => x.FileName == fileName);
        if (!string.IsNullOrEmpty(filePath))
            query = query.Where(x => x.FilePath == filePath);
        if (fileType.HasValue)
            query = query.Where(x => x.FileType == fileType);
        if (!string.IsNullOrEmpty(sha384))
            query = query.Where(x => x.SHA384 == sha384);
        if (!string.IsNullOrEmpty(orderBy))
        {
            query = query.OrderByPropertyName(orderBy, desc);
        }
        else
        {
            query = query.OrderByDescending(x => x.CreateTime);
        }

        var query2 = query.ProjectTo<StaticResourceTableItemModel>(mapper.ConfigurationProvider);

        var r = await query2.PagingAsync(current, pageSize, RequestAborted);
        return r;
    }

    public async Task<bool> UpdateAsync(
        Guid? operatorUserId,
        AddOrEditStaticResourceModel model,
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
        AddOrEditStaticResourceModel model,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var entity = mapper.Map<StaticResource>(model);
        entity.Id = default;
        entity.CreateUserId = createUserId;

        await db.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(CancellationToken.None);
        return true;
    }
}

partial class StaticResourceRepository<TDbContext> // 微服务
{

}