using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions.Abstractions;
using MemoryPack;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;

/// <summary>
/// <see cref="IReadOnlyAppVer"/> 的 Redis 缓存模型
/// </summary>
[MemoryPackable]
public sealed partial record class AppVerRedisModel : IReadOnlyAppVer
{
    /// <inheritdoc/>
    public Guid Id { get; set; }

    /// <inheritdoc/>
    public DateTimeOffset CreationTime { get; set; }

    /// <inheritdoc/>
    public required string Version { get; set; }

    /// <inheritdoc/>
    public required string PrivateKey { get; set; }

    /// <inheritdoc/>
    public bool Disable { get; set; }
}
