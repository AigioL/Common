using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers;

/// <summary>
/// 活跃用户匿名统计缓存模型
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial class ActiveUserAnonymousStatisticCacheModel
{
    [global::MemoryPack.MemoryPackOrder(0)]
    public required ActiveUserRecordModel Model { get; set; }

    /// <summary>
    /// 客户端侧 IP 地址
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public required string IPAddress { get; set; }

    /// <summary>
    /// 设备平台
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public DevicePlatform2 DevicePlatform { get; set; }

    /// <summary>
    /// 设备 Id
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public required string DeviceId { get; set; }

    ///// <summary>
    ///// 客户端版本 Id
    ///// </summary>
    //[global::MemoryPack.MemoryPackOrder(4)]
    //public Guid? ClientVersionId { get; set; }

    ///// <summary>
    ///// 客户端分发渠道
    ///// </summary>
    //[global::MemoryPack.MemoryPackOrder(5)]
    //public ClientDistributionChannel ClientDistributionChannel { get; set; }
}
