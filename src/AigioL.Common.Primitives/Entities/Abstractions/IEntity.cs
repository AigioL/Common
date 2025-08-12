using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace AigioL.Common.Primitives.Entities.Abstractions;

/// <summary>
/// 实体模型接口（数据库表）
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Id，即主键
    /// </summary>
    object Id { get; }

    const DynamicallyAccessedMemberTypes DAMT =
        DynamicallyAccessedMemberTypes.PublicConstructors
        | DynamicallyAccessedMemberTypes.NonPublicConstructors
        | DynamicallyAccessedMemberTypes.PublicProperties
        | DynamicallyAccessedMemberTypes.PublicFields
        | DynamicallyAccessedMemberTypes.NonPublicProperties
        | DynamicallyAccessedMemberTypes.NonPublicFields
        | DynamicallyAccessedMemberTypes.Interfaces;
}

/// <summary>
/// 实体模型主键泛型接口（数据库表）
/// </summary>
/// <typeparam name="TPrimaryKey"></typeparam>
public interface IEntity<[DynamicallyAccessedMembers(DAMT)] TPrimaryKey>
    : IEntity
    where TPrimaryKey : notnull, IEquatable<TPrimaryKey>
{
    /// <inheritdoc cref="IEntity.Id"/>
    new TPrimaryKey Id { get; set; }

    /// <inheritdoc/>
    object IEntity.Id => Id;

    /// <summary>
    /// 判断主键是否为默认值
    /// </summary>
    /// <param name="primaryKey">主键</param>
    /// <returns></returns>
    static bool IsDefault(TPrimaryKey primaryKey)
    {
        if (primaryKey == null) return true; // null is default
        var defPrimaryKey = default(TPrimaryKey);
        if (defPrimaryKey == null) return false; // primaryKey not null
        return EqualityComparer<TPrimaryKey>.Default.Equals(primaryKey, defPrimaryKey);
    }

    /// <summary>
    /// 返回主键匹配表达式
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="primaryKey">主键</param>
    /// <returns></returns>
    static Expression<Func<TEntity, bool>> LambdaEqualId<[DynamicallyAccessedMembers(DAMT)] TEntity>(TPrimaryKey primaryKey)
    {
        var parameter = Expression.Parameter(typeof(TEntity));
        var left = Expression.Property(parameter, typeof(TEntity), nameof(Id));
        var right = Expression.Constant(primaryKey, typeof(TPrimaryKey));
        var body = Expression.Equal(left, right);
        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }
}