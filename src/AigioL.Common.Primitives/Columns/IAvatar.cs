namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 头像
/// </summary>
public interface IAvatar : IReadOnlyAvatar
{
    /// <inheritdoc cref="IAvatar"/>
    new string? Avatar { get; set; }
}

/// <inheritdoc cref="IAvatar"/>
public interface IReadOnlyAvatar
{
    /// <inheritdoc cref="IAvatar"/>
    string Avatar { get; }
}