namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 是否置顶
/// </summary>
public interface IIsTop : IReadOnlyIsTop
{
    /// <inheritdoc cref="IIsTop"/>
    new bool IsTop { get; set; }
}

/// <inheritdoc cref="IIsTop"/>
public interface IReadOnlyIsTop
{
    /// <inheritdoc cref="IIsTop"/>
    bool IsTop { get; }
}
