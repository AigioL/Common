namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 描述
/// </summary>
public interface IDescription : IReadOnlyDescription
{
    /// <inheritdoc cref="IDescription"/>
    new string? Description { get; set; }
}

/// <inheritdoc cref="IDescription"/>
public interface IReadOnlyDescription
{
    /// <inheritdoc cref="IDescription"/>
    string? Description { get; }
}