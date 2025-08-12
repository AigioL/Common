namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 密码
/// </summary>
public interface IPassword : IReadOnlyPassword
{
    /// <inheritdoc cref="IPassword"/>
    new string? Password { get; set; }
}

/// <inheritdoc cref="IPassword"/>
public interface IReadOnlyPassword
{
    /// <inheritdoc cref="IPassword"/>
    string? Password { get; }
}
