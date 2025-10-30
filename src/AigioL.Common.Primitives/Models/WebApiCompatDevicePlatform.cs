using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AigioL.Common.Primitives.Models;

/// <summary>
/// WebApi 定义的平台，用于旧接口兼容，新接口应使用 <see cref="DevicePlatform2"/>
/// </summary>
[Flags]
[Obsolete(
"""
use DevicePlatform2 Or ClientPlatform replace on top of newly added content.
oldType: System.Runtime.Devices.Platform
""")]
public enum WebApiCompatDevicePlatform
{
    /// <summary>
    /// 未知
    /// </summary>
    Unknown = 1,

    /// <summary>
    /// Microsoft Windows(Win32)
    /// </summary>
    Windows = 4,

    /// <summary>
    /// Ubuntu / Debian / CentOS / Tizen
    /// </summary>
    Linux = 8,

    /// <summary>
    /// Android Phone / Android Pad / WearOS(Android Wear) / Android TV
    /// </summary>
    Android = 16,

    /// <summary>
    /// iOS / iPadOS / watchOS / tvOS / macOS
    /// </summary>
    Apple = 32,

    /// <summary>
    /// Universal Windows Platform
    /// </summary>
    [Description("use Win32")]
    UWP = 64,

    /// <summary>
    /// Windows UI 库 (WinUI) 3
    /// </summary>
    WinUI = 128,
}

/// <summary>
/// Enum 扩展 <see cref="WebApiCompatDevicePlatform"/>
/// </summary>
[Obsolete(
"""
use DevicePlatform2 Or ClientPlatform replace on top of newly added content.
""")]
public static partial class WebApiCompatDevicePlatformEnumExtensions
{
    /// <summary>
    /// 值是否在定义的范围中，排除 <see cref="WebApiCompatDevicePlatform.Unknown"/>
    /// </summary>
    /// <param name="platform"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDefined(this WebApiCompatDevicePlatform platform)
        => platform != WebApiCompatDevicePlatform.Unknown &&
#if NET5_0_OR_GREATER
            Enum.IsDefined(platform);
#else
            Enum.IsDefined(typeof(WebApiCompatDevicePlatform), platform);
#endif

    /// <summary>
    /// 将 <see cref="DevicePlatform2"/> 转换为 <see cref="WebApiCompatDevicePlatform"/>
    /// </summary>
    /// <param name="devicePlatform"></param>
    /// <returns></returns>
    public static WebApiCompatDevicePlatform ToWebApiCompat(this DevicePlatform2 devicePlatform) => devicePlatform switch
    {
        DevicePlatform2.UWP => WebApiCompatDevicePlatform.UWP,
        DevicePlatform2.WindowsDesktopBridge or
        DevicePlatform2.Windows => WebApiCompatDevicePlatform.Windows,
        DevicePlatform2.WSA or
        DevicePlatform2.AndroidUnknown or
        DevicePlatform2.AndroidPhone or
        DevicePlatform2.AndroidTablet or
        DevicePlatform2.AndroidDesktop or
        DevicePlatform2.AndroidTV or
        DevicePlatform2.AndroidWatch or
        DevicePlatform2.AndroidVirtual => WebApiCompatDevicePlatform.Android,
        DevicePlatform2.iPadOS or
        DevicePlatform2.iOS or
        DevicePlatform2.tvOS or
        DevicePlatform2.macOS or
        DevicePlatform2.watchOS or
        DevicePlatform2.MacCatalyst => WebApiCompatDevicePlatform.Apple,
        DevicePlatform2.Linux or
        DevicePlatform2.ChromeOS => WebApiCompatDevicePlatform.Linux,
        DevicePlatform2.WinUI => WebApiCompatDevicePlatform.WinUI,
        _ => WebApiCompatDevicePlatform.Unknown,
    };
}