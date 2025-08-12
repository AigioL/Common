namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 是否禁用
/// </summary>
public interface IDisable : IReadOnlyDisable
{
    /// <inheritdoc cref="IDisable"/>
    new bool Disable { get; set; }
}

/// <inheritdoc cref="IDisable"/>
public interface IReadOnlyDisable
{
    /// <inheritdoc cref="IDisable"/>
    bool Disable { get; }
}
