using AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories;

sealed partial class ArticleTagRepository<TDbContext> :
    Repository<TDbContext, ArticleTag, Guid>,
    IArticleTagRepository
    where TDbContext : DbContext, IArticleDbContext
{
    public ArticleTagRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }
}

partial class ArticleTagRepository<TDbContext> // 管理后台
{
    public async Task<PagedModel<ArticleTagTableItemModel>> QueryAsync(
        string? name,
        DateTimeOffset[]? createTime,
        DateTimeOffset[]? updateTime,
        string? createUser,
        string? operatorUser,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.ArticleTags
            .AsNoTrackingWithIdentityResolution();
        if (!string.IsNullOrEmpty(name))
            query = query.Where(x => x.Name.Contains(name));
        if (createTime != null)
            query = createTime.Length switch
            {
                1 => query.Where(x => x.CreateTime >= createTime[0]),
                2 => query.Where(x => x.CreateTime >= createTime[0] && x.CreateTime <= createTime[1]),
                _ => query,
            };
        if (updateTime != null)
            query = updateTime.Length switch
            {
                1 => query.Where(x => x.UpdateTime >= updateTime[0]),
                2 => query.Where(x => x.UpdateTime >= updateTime[0] && x.UpdateTime <= updateTime[1]),
                _ => query,
            };
        if (!string.IsNullOrEmpty(createUser))
            if (ShortGuid.TryParse(createUser, out Guid createUserId))
                query = query.Where(x => x.CreateUser!.Id == createUserId);
            else
                query = query.Where(x => x.CreateUser!.NickName!.Contains(createUser));
        if (!string.IsNullOrEmpty(operatorUser))
            if (ShortGuid.TryParse(operatorUser, out Guid operatorUserId))
                query = query.Where(x => x.OperatorUser!.Id == operatorUserId);
            else
                query = query.Where(x => x.OperatorUser!.NickName!.Contains(operatorUser));
        if (!string.IsNullOrEmpty(orderBy))
        {
            query = query.OrderByPropertyName(orderBy, desc);
        }
        else
        {
            query = query.OrderByDescending(x => x.CreateTime);
        }

        var query2 = query.ProjectTo<ArticleTagTableItemModel>(mapper.ConfigurationProvider);

        var r = await query2.PagingAsync(current, pageSize, cancellationToken);
        return r;
    }

    public async Task<AddOrEditArticleTagModel?> GetEditByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = db.ArticleTags
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == id);

        var query2 = query.Select(static x => new AddOrEditArticleTagModel
        {
            Name = x.Name,
        });

        var r = await query2.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<ApiRsp> UpdateAsync(
        Guid? operatorUserId,
        Guid id,
        AddOrEditArticleTagModel model,
        CancellationToken cancellationToken = default)
    {
        var exists = await ExistsAsync(model.Name, id, cancellationToken: cancellationToken);
        if (exists)
        {
            return "相同的标签名已存在";
        }

        var rowCount = await db.ArticleTags
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(x => x
                .SetProperty(y => y.UpdateTime, y => DateTimeOffset.Now)
                .SetProperty(y => y.OperatorUserId, y => operatorUserId)
                .SetProperty(y => y.Name, y => model.Name),
         CancellationToken.None);
        return rowCount > 0;
    }

    public async Task<ApiRsp> InsertAsync(
        Guid? createUserId,
        AddOrEditArticleTagModel model,
        CancellationToken cancellationToken = default)
    {
        var exists = await ExistsAsync(model.Name, cancellationToken: cancellationToken);
        if (exists)
        {
            return "相同的标签名已存在";
        }

        await db.ArticleTags.AddAsync(new()
        {
            Name = model.Name,
            CreateUserId = createUserId,
        }, CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<Guid[]> InsertAsync(
        Guid? createUserId,
        IReadOnlyCollection<string> tags,
        CancellationToken cancellationToken = default)
    {
        if (tags.Count == 0)
        {
            return [];
        }

        var existingTags = await db.ArticleTags
            .AsNoTrackingWithIdentityResolution()
            .Where(a => tags.Contains(a.Name))
            .ToArrayAsync(cancellationToken);
        var toInsertTags = tags
            .Where(a => !existingTags.Any(e => a == e.Name))
            .Select(a => new ArticleTag()
            {
                Name = a,
                CreateUserId = createUserId,
            }).ToArray();

        await db.ArticleTags.AddRangeAsync(toInsertTags, CancellationToken.None);
        await db.SaveChangesAsync(CancellationToken.None);

        var r = existingTags
            .Select(static x => x.Id)
            .Concat(toInsertTags
                .Select(static x => x.Id))
            .ToArray();
        return r;
    }

    public async Task<ArticleTagOptionItemModel[]> QueryOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.ArticleTags
            .AsNoTrackingWithIdentityResolution()
            .OrderByDescending(x => x.CreateTime)
            .ProjectTo<ArticleTagOptionItemModel>(mapper.ConfigurationProvider);
        var r = await query.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<bool> ExistsAsync(string name, Guid? id = null, CancellationToken cancellationToken = default)
    {
        var query = db.ArticleTags
            .AsNoTrackingWithIdentityResolution();

        bool r;
        if (id.HasValue)
        {
            r = await query.AnyAsync(e => e.Name == name && e.Id != id.Value, cancellationToken);
        }
        else
        {
            r = await query.AnyAsync(e => e.Name == name, cancellationToken);
        }
        return r;
    }
}