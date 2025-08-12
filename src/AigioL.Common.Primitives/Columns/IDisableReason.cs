namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 禁用原因
/// </summary>
public interface IDisableReason : IReadOnlyDisableReason
{
    /// <inheritdoc cref="IDisableReason"/>
    new string? DisableReason { get; set; }
}

/// <inheritdoc cref="IDisableReason"/>
public interface IReadOnlyDisableReason
{
    /// <inheritdoc cref="IDisableReason"/>
    string? DisableReason { get; }
}
