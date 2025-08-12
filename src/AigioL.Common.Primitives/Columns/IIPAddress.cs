namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// IPv4 或 IPv6 地址
/// </summary>
public interface IIPAddress : IReadOnlyIPAddress
{
    /// <inheritdoc cref="IIPAddress"/>
    new string? IPAddress { get; set; }
}

/// <inheritdoc cref="IIPAddress"/>
public interface IReadOnlyIPAddress
{
    /// <inheritdoc cref="IIPAddress"/>
    string? IPAddress { get; }
}