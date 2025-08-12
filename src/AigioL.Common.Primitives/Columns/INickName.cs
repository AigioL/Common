namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 昵称
/// </summary>
public interface INickName : IReadOnlyNickName
{
    /// <inheritdoc cref="INickName"/>
    new string? NickName { get; set; }
}

public interface IReadOnlyNickName
{
    /// <inheritdoc cref="INickName"/>
    string? NickName { get; }
}
