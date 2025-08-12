using AigioL.Common.Primitives.Entities.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 主键
/// </summary>
/// <typeparam name="TPrimaryKey"></typeparam>
public interface IId<[DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey> : IReadOnlyId<TPrimaryKey>
{
    /// <inheritdoc cref="IId{TPrimaryKey}"/>
    new TPrimaryKey Id { get; set; }
}

/// <inheritdoc cref="IId{TPrimaryKey}"/>
public interface IReadOnlyId<[DynamicallyAccessedMembers(IEntity.DAMT)] TPrimaryKey> : IReadOnlyId
{
    /// <inheritdoc cref="IId{TPrimaryKey}"/>
    new TPrimaryKey Id { get; }

    /// <inheritdoc/>
    object? IReadOnlyId.Id => this.Id;
}

/// <inheritdoc cref="IId{TPrimaryKey}"/>
public interface IReadOnlyId
{
    /// <inheritdoc cref="IId{TPrimaryKey}"/>
    object? Id { get; }
}