using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.EntityFrameworkCore.Helpers;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

/// <inheritdoc cref="Repository{TDbContext}"/>
public abstract class Repository<TDbContext, [DynamicallyAccessedMembers(IEntity.DAMT)] TEntity> : Repository<TDbContext>, IRepository<TEntity>, IEFRepository<TEntity>, IRequestAbortedProvider
    where TDbContext : DbContext
    where TEntity : class
{
    /// <inheritdoc/>
    public DbSet<TEntity> Entity { get; }

    /// <inheritdoc/>
    public IQueryable<TEntity> EntityNoTracking => Entity.AsNoTrackingWithIdentityResolution();

    /// <inheritdoc/>
    DbContext IEFRepository.DbContext => db;

    string? mTableName;
    IEntityType? mEntityType;
    IHttpContextAccessor? httpContextAccessor;
    readonly IRequestAbortedProvider? requestAbortedProvider;

    /// <inheritdoc/>
    public string TableName
    {
        get
        {
            mTableName ??= db.Database.GetTableNameByClrType(EntityType);
            return mTableName;
        }
    }

    /// <summary>
    /// 获取实体类型
    /// </summary>
    public IEntityType EntityType
    {
        get
        {
            if (mEntityType == null)
            {
                var entityType = db.Model.FindEntityType(typeof(TEntity));
                ArgumentNullException.ThrowIfNull(entityType);
                mEntityType = entityType;
            }
            return mEntityType;
        }
    }

    [DynamicallyAccessedMembers(IEntity.DAMT)]
    static readonly Type entityType = typeof(TEntity);

    static bool? mIsSoftDeleted;

    /// <summary>
    /// 是否软删除
    /// </summary>
    public static bool IsSoftDeleted
    {
        get
        {
            mIsSoftDeleted ??= ColumnHelper.IsSoftDeleted(entityType);
            return mIsSoftDeleted.Value;
        }
    }

    /// <summary>
    /// 获取请求中断的令牌
    /// </summary>
    protected virtual CancellationToken RequestAborted
    {
        get
        {
            if (requestAbortedProvider != null)
            {
                return requestAbortedProvider.RequestAborted;
            }
            else
            {
                var httpContext = httpContextAccessor?.HttpContext;
                if (httpContext != null)
                {
                    return httpContext.RequestAborted;
                }
            }
            return default;
        }
    }

    /// <inheritdoc cref="RequestAborted"/>
    CancellationToken IEFRepository<TEntity>.RequestAborted => RequestAborted;

    public Repository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext)
    {
        Entity = dbContext.Set<TEntity>();
        requestAbortedProvider = serviceProvider.GetService<IRequestAbortedProvider>();
        if (requestAbortedProvider == null)
        {
            httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
        }
    }

    #region 增(Insert Funs) 立即执行并返回受影响的行数

    /// <inheritdoc/>
    public virtual async Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Entity.AddAsync(entity, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<int> InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await Entity.AddRangeAsync(entities, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region 删(Delete Funs) 立即执行并返回受影响的行数

    /// <inheritdoc/>
    public virtual async Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (IsSoftDeleted && entity is ISoftDeleted softDeleted)
        {
            softDeleted.SoftDeleted = true;
            Entity.Update(entity);
        }
        else
        {
            Entity.Remove(entity);
        }
        return await db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<int> DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (IsSoftDeleted && entities is IEnumerable<ISoftDeleted> softDeleted)
        {
            foreach (var item in softDeleted)
            {
                item.SoftDeleted = true;
            }
            Entity.UpdateRange(entities);
        }
        else
        {
            Entity.RemoveRange(entities);
        }
        return await db.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region 改(Update Funs) 立即执行并返回受影响的行数

    /// <inheritdoc/>
    public virtual async Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Entity.Update(entity);
        return await db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<int> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        Entity.UpdateRange(entities);
        return await db.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region 查(通用查询)

    /// <inheritdoc/>
    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await Entity.FirstOrDefaultAsync(predicate, cancellationToken);

    #endregion
}

