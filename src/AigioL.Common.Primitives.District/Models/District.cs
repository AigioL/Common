using AigioL.Common.Primitives.Models.Abstractions;
using MemoryPack;
using System.Diagnostics;
using R = AigioL.Common.Primitives.District.Properties.Resources;

namespace AigioL.Common.Primitives.Models;

/// <inheritdoc cref="IDistrict"/>
[global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public sealed partial class District : IDistrict
{
    string DebuggerDisplay() => $"{Name}, {Id}";

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(0)]
    public int Id { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string? Name { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(2)]
    public DistrictLevel Level { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(3)]
    public int? Up { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(4)]
    public string? ShortName { get; set; }

    /// <inheritdoc/>
    public override string ToString() => IDistrict.ToString(this);

    static readonly Lazy<District[]> all = new(() =>
    {
        using var stream = R.AMap_adcode_citycode;
        var t = MemoryPackSerializer.DeserializeAsync<District[]>(stream);
        return t.Result;
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// 获取所有行政区域数据
    /// </summary>
    public static District[] All => all.Value;
}