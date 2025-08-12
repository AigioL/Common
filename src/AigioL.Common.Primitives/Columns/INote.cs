namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 备注
/// </summary>
public interface INote : IReadOnlyNote
{
    /// <inheritdoc cref="INote"/>
    new string? Note { get; set; }
}

/// <inheritdoc cref="INote"/>
public interface IReadOnlyNote
{
    /// <inheritdoc cref="INote"/>
    string? Note { get; }
}
