namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 是否软删除
/// </summary>
public interface ISoftDeleted : IReadOnlySoftDeleted
{
    /// <inheritdoc cref="ISoftDeleted"/>
    new bool SoftDeleted { get; set; }
}

/// <inheritdoc cref="ISoftDeleted"/>
public interface IReadOnlySoftDeleted
{
    /// <inheritdoc cref="ISoftDeleted"/>
    bool SoftDeleted { get; }
}
