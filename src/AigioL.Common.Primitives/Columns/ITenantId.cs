namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 租户 Id
/// </summary>
public interface ITenantId : IReadOnlyTenantId
{
    /// <inheritdoc cref="ITenantId"/>
    new Guid TenantId { get; set; }
}

/// <inheritdoc cref="ITenantId"/>
public interface IReadOnlyTenantId
{
    /// <inheritdoc cref="ITenantId"/>
    Guid TenantId { get; }
}