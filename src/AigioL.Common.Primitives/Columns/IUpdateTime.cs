namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 更新时间
/// </summary>
public interface IUpdateTime : IReadOnlyUpdateTime
{
    /// <inheritdoc cref="IUpdateTime"/>
    new DateTimeOffset UpdateTime { get; set; }
}

/// <inheritdoc cref="IUpdateTime"/>
public interface IReadOnlyUpdateTime
{
    /// <inheritdoc cref="IUpdateTime"/>
    DateTimeOffset UpdateTime { get; }
}