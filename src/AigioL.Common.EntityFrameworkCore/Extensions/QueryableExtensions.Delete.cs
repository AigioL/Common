using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AigioL.Common.EntityFrameworkCore.Extensions;

static partial class QueryableExtensions
{
    #region 根据条件执行删除

    /// <summary>
    /// 根据条件常规删除（批量删除或软删除），根据实体是否继承了 <see cref="ISoftDeleted"/> 自动检测
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<int> GeneralDeleteAsync<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity>(
        this IQueryable<TEntity> query,
        Guid? operatorUserId = null,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
    {
        if (query is not IQueryable<ISoftDeleted> query2)
        {
            var r = await query.ExecuteDeleteAsync(cancellationToken);
            return r;
        }
        else
        {
            var r = await query2.SoftDeleteAsync(operatorUserId, cancellationToken);
            return r;
        }
    }

    /// <summary>
    /// 根据条件批量软删除
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<int> SoftDeleteAsync(
        this IQueryable<ISoftDeleted> query,
        Guid? operatorUserId = null,
        CancellationToken cancellationToken = default)
    {
        if (query is IQueryable<IOperatorUserId> query2)
        {
            var r = await query.ExecuteUpdateAsync(s => s
                .SetProperty(b => b.DeleteTime, DateTimeOffset.Now)
                .SetProperty(b => EF.Property<Guid?>(b, nameof(IOperatorUserId.OperatorUserId)), operatorUserId)
                , cancellationToken);

            //var t = (Task<int>)typeof(QueryableExtensions)
            //    .GetMethod(nameof(_SoftDeleteCoreAsync), BindingFlags.Static | BindingFlags.NonPublic)
            //    .MakeGenericMethod(query.GetType().GenericTypeArguments[0])
            //    .Invoke(null, [query, operatorUserId, cancellationToken]);
            //var r = await t;
            return r;
        }
        else
        {
            var r = await query.ExecuteUpdateAsync(s => s.SetProperty(b => b.DeleteTime, DateTimeOffset.Now), cancellationToken);
            return r;
        }
    }

    //#pragma warning disable IDE1006 // 命名样式
    //    static async Task<int> _SoftDeleteCoreAsync<T>(
    //#pragma warning restore IDE1006 // 命名样式
    //        this IQueryable<T> query,
    //        Guid? operatorUserId = null,
    //        CancellationToken cancellationToken = default)
    //        where T : IOperatorUserId, ISoftDeleted
    //    {
    //        var r = await query.ExecuteUpdateAsync(s => s
    //            .SetProperty(b => b.DeleteTime, DateTimeOffset.Now)
    //            .SetProperty(b => b.OperatorUserId, operatorUserId)
    //            , cancellationToken);
    //        return r;
    //    }

    #endregion

    #region 根据主键进行删除

    /// <summary>
    /// 根据主键常规删除（批量删除或软删除），根据实体是否继承了 <see cref="ISoftDeleted"/> 自动检测
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<int> GeneralDeleteByIdAsync<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity, [DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey>(
        this IQueryable<TEntity> query,
        TPrimaryKey primaryKey,
        Guid? operatorUserId = null,
        CancellationToken cancellationToken = default)
        where TPrimaryKey : IEquatable<TPrimaryKey>
        where TEntity : class, IEntity
    {
        query = query.Where(IEntity<TPrimaryKey>.LambdaEqualId<TEntity>(primaryKey));
        var r = await query.GeneralDeleteAsync(operatorUserId, cancellationToken);
        return r;
    }

    /// <summary>
    /// 根据主键软删除
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<int> SoftDeleteByIdAsync<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity, [DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey>(
        this IQueryable<TEntity> query,
        TPrimaryKey primaryKey,
        Guid? operatorUserId = null,
        CancellationToken cancellationToken = default)
        where TPrimaryKey : IEquatable<TPrimaryKey>
        where TEntity : class, IEntity, ISoftDeleted
    {
        query = query.Where(IEntity<TPrimaryKey>.LambdaEqualId<TEntity>(primaryKey));
        var r = await query.SoftDeleteAsync(operatorUserId, cancellationToken);
        return r;
    }

    /// <summary>
    /// 根据主键硬删除（从数据库中删除数据）
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TPrimaryKey"></typeparam>
    /// <param name="query"></param>
    /// <param name="primaryKey"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<int> HardDeleteByIdAsync<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity, [DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey>(
        this IQueryable<TEntity> query,
        TPrimaryKey primaryKey,
        CancellationToken cancellationToken = default)
        where TPrimaryKey : IEquatable<TPrimaryKey>
        where TEntity : class, IEntity, ISoftDeleted
    {
        query = query.Where(IEntity<TPrimaryKey>.LambdaEqualId<TEntity>(primaryKey));
        var r = await query.ExecuteDeleteAsync(cancellationToken);
        return r;
    }

    #endregion

    #region 根据多个主键进行删除

    /// <summary>
    /// 根据多个主键常规删除（批量删除或软删除），根据实体是否继承了 <see cref="ISoftDeleted"/> 自动检测
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<int> GeneralDeleteByIdAsync<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity, [DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey>(
        this IQueryable<TEntity> query,
        IEnumerable<TPrimaryKey> primaryKeys,
        Guid? operatorUserId = null,
        CancellationToken cancellationToken = default)
        where TPrimaryKey : IEquatable<TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
    {
        query = query.Where(x => Enumerable.Contains(primaryKeys, x.Id));
        var r = await query.GeneralDeleteAsync(operatorUserId, cancellationToken);
        return r;
    }

    /// <summary>
    /// 根据多个主键软删除
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<int> SoftDeleteByIdAsync<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity, [DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey>(
        this IQueryable<TEntity> query,
        IEnumerable<TPrimaryKey> primaryKeys,
        Guid? operatorUserId = null,
        CancellationToken cancellationToken = default)
        where TPrimaryKey : IEquatable<TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>, ISoftDeleted
    {
        query = query.Where(x => Enumerable.Contains(primaryKeys, x.Id));
        var r = await query.SoftDeleteAsync(operatorUserId, cancellationToken);
        return r;
    }

    /// <summary>
    /// 根据多个主键硬删除（从数据库中删除数据）
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TPrimaryKey"></typeparam>
    /// <param name="query"></param>
    /// <param name="primaryKeys"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<int> HardDeleteByIdAsync<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity, [DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey>(
        this IQueryable<TEntity> query,
        IEnumerable<TPrimaryKey> primaryKeys,
        CancellationToken cancellationToken = default)
        where TPrimaryKey : IEquatable<TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>, ISoftDeleted
    {
        query = query.Where(x => Enumerable.Contains(primaryKeys, x.Id));
        var r = await query.ExecuteDeleteAsync(cancellationToken);
        return r;
    }

    #endregion

    #region 根据实体进行删除

    /// <summary>
    /// 根据实体常规删除（批量删除或软删除），根据实体是否继承了 <see cref="ISoftDeleted"/> 自动检测
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<int> GeneralDeleteAsync<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity, [DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey>(
        this IQueryable<TEntity> query,
        TEntity entity,
        Guid? operatorUserId = null,
        CancellationToken cancellationToken = default)
        where TPrimaryKey : IEquatable<TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
    {
        var r = await query.GeneralDeleteByIdAsync(entity.Id, operatorUserId, cancellationToken);
        return r;
    }

    /// <summary>
    /// 根据实体软删除
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<int> SoftDeleteAsync<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity, [DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey>(
        this IQueryable<TEntity> query,
        TEntity entity,
        Guid? operatorUserId = null,
        CancellationToken cancellationToken = default)
        where TPrimaryKey : IEquatable<TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>, ISoftDeleted
    {
        var r = await query.SoftDeleteByIdAsync(entity.Id, operatorUserId, cancellationToken);
        return r;
    }

    /// <summary>
    /// 根据实体硬删除（从数据库中删除数据）
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TPrimaryKey"></typeparam>
    /// <param name="query"></param>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<int> HardDeleteAsync<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity, [DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey>(
        this IQueryable<TEntity> query,
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TPrimaryKey : IEquatable<TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>, ISoftDeleted
    {
        var r = await query.HardDeleteByIdAsync(entity.Id, cancellationToken);
        return r;
    }

    #endregion

    #region 根据多个实体进行删除

    /// <summary>
    /// 根据多个实体常规删除（批量删除或软删除），根据实体是否继承了 <see cref="ISoftDeleted"/> 自动检测
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<int> GeneralDeleteAsync<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity, [DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey>(
        this IQueryable<TEntity> query,
        IEnumerable<TEntity> entities,
        Guid? operatorUserId = null,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TPrimaryKey>
        where TPrimaryKey : IEquatable<TPrimaryKey>
    {
        var r = await query.GeneralDeleteByIdAsync(entities.Select(x => x.Id), operatorUserId, cancellationToken);
        return r;
    }

    /// <summary>
    /// 根据多个实体软删除
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<int> SoftDeleteAsync<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity, [DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey>(
        this IQueryable<TEntity> query,
        IEnumerable<TEntity> entities,
        Guid? operatorUserId = null,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity<TPrimaryKey>, ISoftDeleted
        where TPrimaryKey : IEquatable<TPrimaryKey>
    {
        var r = await query.SoftDeleteByIdAsync(entities.Select(x => x.Id), operatorUserId, cancellationToken);
        return r;
    }

    /// <summary>
    /// 根据多个实体硬删除（从数据库中删除数据）
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TPrimaryKey"></typeparam>
    /// <param name="query"></param>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<int> HardDeleteAsync<[DynamicallyAccessedMembers(IEntity.DAMT)] TEntity, [DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey>(
        this IQueryable<TEntity> query,
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
        where TPrimaryKey : IEquatable<TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>, ISoftDeleted
    {
        var r = await query.HardDeleteByIdAsync(entities.Select(x => x.Id), cancellationToken);
        return r;
    }

    #endregion
}
