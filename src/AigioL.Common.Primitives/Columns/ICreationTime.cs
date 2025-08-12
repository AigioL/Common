namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 创建时间
/// </summary>
public interface ICreationTime : IReadOnlyCreationTime
{
    /// <inheritdoc cref="ICreationTime"/>
    new DateTimeOffset CreationTime { get; set; }
}

/// <inheritdoc cref="ICreationTime"/>
public interface IReadOnlyCreationTime
{
    /// <inheritdoc cref="ICreationTime"/>
    DateTimeOffset CreationTime { get; }
}