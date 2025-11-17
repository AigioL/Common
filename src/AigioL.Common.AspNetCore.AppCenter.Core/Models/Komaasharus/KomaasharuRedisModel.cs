using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus;

[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class KomaasharuRedisModel
{
    /// <summary>
    /// 广告 Id
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public Guid Id { get; set; }

    /// <summary>
    /// 排序值
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public long Sort { get; set; }

    /// <summary>
    /// 广告描述
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public string? Desc { get; set; }

    /// <inheritdoc cref="KomaasharuType"/>
    [global::MemoryPack.MemoryPackOrder(3)]
    public KomaasharuType Type { get; set; }

    /// <inheritdoc cref="KomaasharuOrientation"/>
    [global::MemoryPack.MemoryPackOrder(4)]
    public KomaasharuOrientation Orientation { get; set; }

    [global::MemoryPack.MemoryPackOrder(5)]
    public required string Name { get; set; }

    [global::MemoryPack.MemoryPackOrder(6)]
    public bool IsAuth { get; set; }

    [global::MemoryPack.MemoryPackOrder(7)]
#pragma warning disable CS0618 // 类型或成员已过时
    public WebApiCompatDevicePlatform Platform { get; set; }
#pragma warning restore CS0618 // 类型或成员已过时

    [global::MemoryPack.MemoryPackOrder(8)]
    public DeviceIdiom DeviceIdiom { get; set; }

    [global::MemoryPack.MemoryPackOrder(9)]
    public string? ImageUrl { get; set; }

    [global::MemoryPack.MemoryPackOrder(10)]
    public string? JumpUrl { get; set; }
}
