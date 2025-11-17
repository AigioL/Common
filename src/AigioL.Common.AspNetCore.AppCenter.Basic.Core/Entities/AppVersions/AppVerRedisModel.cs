using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using MemoryPack;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;

/// <summary>
/// <see cref="IReadOnlyAppVer"/> 的 Redis 缓存模型
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class AppVerRedisModel : IReadOnlyAppVer
{
    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(0)]
    public Guid Id { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(1)]
    public DateTimeOffset CreationTime { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(2)]
    public required string Version { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(3)]
    public required string PrivateKey { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(4)]
    public bool Disable { get; set; }
}
