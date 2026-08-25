namespace AigioL.Common.Net.ReverseProxy.Models;

/// <summary>
/// 流量统计模型类
/// </summary>
#if !DISABLE_MP2
[global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
#endif
public sealed partial record class FlowStatistics
{
    /// <summary>
    /// 获取总读上行
    /// </summary>
#if !DISABLE_MP2
    [global::MemoryPack.MemoryPackOrder(0)]
#endif
    public long TotalRead { get; init; }

    /// <summary>
    /// 获取总下行
    /// </summary>
#if !DISABLE_MP2
    [global::MemoryPack.MemoryPackOrder(1)]
#endif
    public long TotalWrite { get; init; }

    /// <summary>
    /// 获取读取速率
    /// </summary>
#if !DISABLE_MP2
    [global::MemoryPack.MemoryPackOrder(2)]
#endif
    public double ReadRate { get; init; }

    /// <summary>
    /// 获取写入速率
    /// </summary>
#if !DISABLE_MP2
    [global::MemoryPack.MemoryPackOrder(3)]
#endif
    public double WriteRate { get; init; }
}
