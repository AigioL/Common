namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 创建时间
/// </summary>
public interface ICreateTime : IReadOnlyCreateTime
{
    /// <inheritdoc cref="ICreateTime"/>
    new DateTimeOffset CreateTime { get; set; }
}

/// <inheritdoc cref="ICreateTime"/>
public interface IReadOnlyCreateTime
{
    /// <inheritdoc cref="ICreateTime"/>
    DateTimeOffset CreateTime { get; }
}