namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 标题
/// </summary>
public interface ITitle : IReadOnlyTitle
{
    /// <inheritdoc cref="ITitle"/>
    new string? Title { get; set; }
}

/// <inheritdoc cref="ITitle"/>
public interface IReadOnlyTitle
{
    /// <inheritdoc cref="ITitle"/>
    string? Title { get; }
}
