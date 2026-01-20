namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 是否软删除
/// </summary>
public interface ISoftDeleted : IReadOnlySoftDeleted
{
    /// <inheritdoc cref="ISoftDeleted"/>
    new DateTimeOffset? DeleteTime { get; set; }
}

/// <inheritdoc cref="ISoftDeleted"/>
public interface IReadOnlySoftDeleted
{
    /// <inheritdoc cref="ISoftDeleted"/>
    DateTimeOffset? DeleteTime { get; }
}
