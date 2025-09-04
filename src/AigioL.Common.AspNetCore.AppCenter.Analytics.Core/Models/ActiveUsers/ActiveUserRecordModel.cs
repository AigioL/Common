using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.Primitives.Models;
using System.Runtime.InteropServices;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers;

/// <summary>
/// (匿名)活跃用户记录，用于统计分析以改进体验，详情见使用协议声明
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial class ActiveUserRecordModel : IExplicitHasValue, IDeviceId
{
    [global::MemoryPack.MemoryPackOrder(0)]
    public ActiveUserAnonymousStatisticType Type { get; set; }

    /// <summary>
    /// 使用平台
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
#pragma warning disable CS0618 // 类型或成员已过时
    public WebApiCompatDevicePlatform Platform { get; set; }
#pragma warning restore CS0618 // 类型或成员已过时

    /// <summary>
    /// 设备类型
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public DeviceIdiom DeviceIdiom { get; set; }

    /// <summary>
    /// 系统版本号
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public string? OSVersion { get; set; }

    /// <summary>
    /// 屏幕总数
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(4)]
    public int ScreenCount { get; set; }

    /// <summary>
    /// 主屏幕像素密度
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(5)]
    public double PrimaryScreenPixelDensity { get; set; }

    /// <summary>
    /// 主屏幕宽度
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(6)]
    public int PrimaryScreenWidth { get; set; }

    /// <summary>
    /// 主屏幕高度
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(7)]
    public int PrimaryScreenHeight { get; set; }

    /// <summary>
    /// 总屏幕宽度
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(8)]
    public int SumScreenWidth { get; set; }

    /// <summary>
    /// 总屏幕高度
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(9)]
    public int SumScreenHeight { get; set; }

    /// <summary>
    /// 当前进程 CPU 架构(处理器体系结构)
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(10)]
    public ArchitectureFlags ProcessArch { get; set; }

    /// <summary>
    /// 是否已登录账号
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(11)]
    public bool? IsAuthenticated { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(12)]
    public Guid DeviceIdG { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(13)]
    public string? DeviceIdR { get; set; }

    /// <inheritdoc/>
    [global::MemoryPack.MemoryPackOrder(14)]
    public string? DeviceIdN { get; set; }

    /// <inheritdoc/>
    bool IExplicitHasValue.ExplicitHasValue()
    {
        return Enum.IsDefined(Type) &&
#pragma warning disable CS0618 // 类型或成员已过时
            Platform.IsDefined() &&
#pragma warning restore CS0618 // 类型或成员已过时
            DeviceIdiom.IsDefined() &&
            !string.IsNullOrWhiteSpace(OSVersion) &&
            Enum.IsDefined(ProcessArch);
    }
}
