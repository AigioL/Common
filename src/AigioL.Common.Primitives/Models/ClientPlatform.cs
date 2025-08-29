using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AigioL.Common.Primitives.Models;

/// <summary>
/// 客户端平台
/// </summary>
[Flags]
public enum ClientPlatform : long
{
    /// <summary>
    /// Microsoft Windows(Win32) 32 位应用程序(x86)
    /// </summary>
    Win32X86 = 1L << 0,

    /// <summary>
    /// Microsoft Windows(Win32) 64 位应用程序(x86-64/x64/AMD64)
    /// </summary>
    Win32X64 = 1L << 1,

    /// <summary>
    /// Microsoft Windows(Win32) ARM 64 位应用程序(ARM64)
    /// </summary>
    Win32Arm64 = 1L << 2,

    /// <summary>
    /// Apple macOS 64 位应用程序(x86-64/x64/AMD64)
    /// </summary>
    macOSX64 = 1L << 3,

    /// <summary>
    /// Apple macOS ARM 64 位应用程序(M1/M2/ARM64)
    /// </summary>
    macOSArm64 = 1L << 4,

    /// <summary>
    /// Ubuntu / Debian / CentOS 64 位应用程序(x86-64/x64/AMD64)
    /// </summary>
    LinuxX64 = 1L << 5,

    /// <summary>
    /// Ubuntu / Debian / CentOS 32 位应用程序(x86)
    /// </summary>
    LinuxX86 = 1L << 6,

    /// <summary>
    /// Ubuntu / Debian / CentOS ARM 64 位应用程序(ARM64)
    /// </summary>
    LinuxArm64 = 1L << 7,

    /// <summary>
    /// Ubuntu / Debian / CentOS ARM 32 位应用程序(ARM)
    /// </summary>
    LinuxArm = 1L << 8,

    /// <summary>
    /// Android 64 位应用程序(x86-64/x64/AMD64/x86_64) for Phone
    /// </summary>
    AndroidPhoneX64 = 1L << 9,

    /// <summary>
    /// Android 32 位应用程序(x86) for Phone
    /// </summary>
    AndroidPhoneX86 = 1L << 10,

    /// <summary>
    /// Android ARM 64 位应用程序(ARM64/arm64-v8a) for Phone
    /// </summary>
    AndroidPhoneArm64 = 1L << 11,

    /// <summary>
    /// Android ARM 32 位应用程序(ARM/armeabi-v7a) for Phone
    /// </summary>
    AndroidPhoneArm = 1L << 12,

    /// <summary>
    /// iOS ARM 64 位应用程序(ARM64/arm64-v8a)
    /// </summary>
    iOSArm64 = 1L << 13,

    /// <summary>
    /// iPadOS ARM 64 位应用程序(ARM64/arm64-v8a)
    /// </summary>
    iPadOSArm64 = 1L << 14,

    /// <summary>
    /// watchOS ARM 64 位应用程序(ARM64/arm64-v8a)
    /// </summary>
    watchOSArm64 = 1L << 15,

    /// <summary>
    /// tvOS ARM 64 位应用程序(ARM64/arm64-v8a)
    /// </summary>
    tvOSArm64 = 1L << 16,

    /// <summary>
    /// Android 64 位应用程序(x86-64/x64/AMD64/x86_64) for Pad
    /// </summary>
    AndroidPadX64 = 1L << 17,

    /// <summary>
    /// Android 32 位应用程序(x86) for Pad
    /// </summary>
    AndroidPadX86 = 1L << 18,

    /// <summary>
    /// Android ARM 64 位应用程序(ARM64/arm64-v8a) for Pad
    /// </summary>
    AndroidPadArm64 = 1L << 19,

    /// <summary>
    /// Android ARM 32 位应用程序(ARM/armeabi-v7a) for Pad
    /// </summary>
    AndroidPadArm = 1L << 20,

    /// <summary>
    /// Android ARM 64 位应用程序(ARM64/arm64-v8a) for Wear
    /// </summary>
    AndroidWearArm64 = 1L << 21,

    /// <summary>
    /// Android 64 位应用程序(x86-64/x64/AMD64/x86_64) for TV
    /// </summary>
    AndroidTVX64 = 1L << 22,

    /// <summary>
    /// Android 32 位应用程序(x86) for TV
    /// </summary>
    AndroidTVX86 = 1L << 23,

    /// <summary>
    /// Android ARM 64 位应用程序(ARM64/arm64-v8a) for TV
    /// </summary>
    AndroidTVArm64 = 1L << 24,

    /// <summary>
    /// Android ARM 32 位应用程序(ARM/armeabi-v7a) for TV
    /// </summary>
    AndroidTVArm = 1L << 25,

    /// <summary>
    /// Universal Windows Platform 32 位应用程序(x86)
    /// </summary>
    UWPX86 = 1L << 26,

    /// <summary>
    /// Universal Windows Platform 64 位应用程序(x86-64/x64/AMD64)
    /// </summary>
    UWPX64 = 1L << 27,

    /// <summary>
    /// Universal Windows Platform ARM 64 位应用程序(ARM64)
    /// </summary>
    UWPArm64 = 1L << 28,

    /// <summary>
    /// Microsoft Store(Win32) 32 位应用程序(x86)
    /// </summary>
    Win32StoreX86 = 1L << 29,

    /// <summary>
    /// Microsoft Store(Win32) 64 位应用程序(x86-64/x64/AMD64)
    /// </summary>
    Win32StoreX64 = 1L << 30,

    /// <summary>
    /// Microsoft Store(Win32) ARM 64 位应用程序(ARM64)
    /// </summary>
    Win32StoreArm64 = 1L << 31,

    /// <summary>
    /// Linux LoongArch 64 位应用程序(LoongArch64)
    /// </summary>
    LinuxLoongArch64 = 1L << 32,

    /// <summary>
    /// Linux LoongArch 32 位应用程序(LoongArch32)
    /// </summary>
    LinuxLoongArch32 = 1L << 33,
}

/// <summary>
/// Enum 扩展 <see cref="ClientPlatform"/>
/// </summary>
public static partial class ClientPlatformEnumExtensions
{
    /// <summary>
    /// 将 Flags 枚举进行拆分
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static IEnumerable<TEnum> EnumFlagsSplit<TEnum>(TEnum value)
        where TEnum : struct, Enum
        => [.. Enum.GetValues<TEnum>().Where(x => value.HasFlag(x))];

    #region Platform

    /// <summary>
    /// Microsoft Windows(Win32)
    /// </summary>
    public const ClientPlatform Platform_Windows =
        ClientPlatform.Win32X86 |
        ClientPlatform.Win32X64 |
        ClientPlatform.Win32Arm64 |
        ClientPlatform.Win32StoreX86 |
        ClientPlatform.Win32StoreX64 |
        ClientPlatform.Win32StoreArm64;

    /// <summary>
    /// Windows Store / MSIX
    /// </summary>
    public const ClientPlatform Platform_Windows_OnlyStore =
        ClientPlatform.Win32StoreX86 |
        ClientPlatform.Win32StoreX64 |
        ClientPlatform.Win32StoreArm64;

    /// <summary>
    /// Ubuntu / Debian / CentOS
    /// </summary>
    public const ClientPlatform Platform_Linux =
        ClientPlatform.LinuxX64 |
        ClientPlatform.LinuxArm64 |
        ClientPlatform.LinuxArm |
        ClientPlatform.LinuxLoongArch64;

    /// <summary>
    /// Android Phone / Android Pad / WearOS(Android Wear) / Android TV
    /// </summary>
    public const ClientPlatform Platform_Android =
        ClientPlatform.AndroidPhoneX64 |
        ClientPlatform.AndroidPhoneX86 |
        ClientPlatform.AndroidPhoneArm64 |
        ClientPlatform.AndroidPhoneArm |
        ClientPlatform.AndroidPadX64 |
        ClientPlatform.AndroidPadX86 |
        ClientPlatform.AndroidPadArm64 |
        ClientPlatform.AndroidPadArm |
        ClientPlatform.AndroidWearArm64 |
        ClientPlatform.AndroidTVX64 |
        ClientPlatform.AndroidTVX86 |
        ClientPlatform.AndroidTVArm64 |
        ClientPlatform.AndroidTVArm;

    /// <summary>
    /// iOS / iPadOS / watchOS / tvOS / macOS
    /// </summary>
    public const ClientPlatform Platform_Apple =
        ClientPlatform.macOSX64 |
        ClientPlatform.macOSArm64 |
        ClientPlatform.iOSArm64 |
        ClientPlatform.iPadOSArm64 |
        ClientPlatform.watchOSArm64 |
        ClientPlatform.tvOSArm64;

    /// <summary>
    /// UWP (Universal Windows Platform)
    /// </summary>
    public const ClientPlatform Platform_UWP =
        ClientPlatform.UWPX86 |
        ClientPlatform.UWPX64 |
        ClientPlatform.UWPArm64;

    #endregion Platform

    #region ArchitectureFlags

    /// <summary>
    /// Intel/AMD 的 32 位处理器体系结构
    /// </summary>
    public const ClientPlatform ArchitectureFlags_X86 =
        ClientPlatform.Win32X86 |
        ClientPlatform.AndroidPhoneX86 |
        ClientPlatform.AndroidPadX86 |
        ClientPlatform.AndroidTVX86 |
        ClientPlatform.UWPX86 |
        ClientPlatform.Win32StoreX86;

    /// <summary>
    /// 32 位 ARM 处理器体系结构
    /// </summary>
    public const ClientPlatform ArchitectureFlags_Arm =
        ClientPlatform.LinuxArm |
        ClientPlatform.AndroidPhoneArm |
        ClientPlatform.AndroidPadArm |
        ClientPlatform.AndroidTVArm;

    /// <summary>
    /// Intel/AMD 的 64 位处理器体系结构(AMD64)
    /// </summary>
    public const ClientPlatform ArchitectureFlags_X64 =
        ClientPlatform.Win32X64 |
        ClientPlatform.macOSX64 |
        ClientPlatform.LinuxX64 |
        ClientPlatform.AndroidPhoneX64 |
        ClientPlatform.AndroidPadX64 |
        ClientPlatform.AndroidTVX64 |
        ClientPlatform.UWPX64 |
        ClientPlatform.Win32StoreX64;

    /// <summary>
    /// 64 位 ARM 处理器体系结构
    /// </summary>
    public const ClientPlatform ArchitectureFlags_Arm64 =
        ClientPlatform.Win32Arm64 |
        ClientPlatform.macOSArm64 |
        ClientPlatform.LinuxArm64 |
        ClientPlatform.AndroidPhoneArm64 |
        ClientPlatform.iOSArm64 |
        ClientPlatform.iPadOSArm64 |
        ClientPlatform.watchOSArm64 |
        ClientPlatform.tvOSArm64 |
        ClientPlatform.AndroidPadArm64 |
        ClientPlatform.AndroidWearArm64 |
        ClientPlatform.AndroidTVArm64 |
        ClientPlatform.UWPArm64 |
        ClientPlatform.Win32StoreArm64;

    /// <summary>
    /// LoongArch 64 位处理器体系结构
    /// </summary>
    public const ClientPlatform ArchitectureFlags_LoongArch64 =
        ClientPlatform.LinuxLoongArch64;

    #endregion ArchitectureFlags

    #region DeviceIdiom

    /// <summary>
    /// 手机
    /// </summary>
    public const ClientPlatform DeviceIdiom_Phone =
        ClientPlatform.AndroidPhoneX64 |
        ClientPlatform.AndroidPhoneX86 |
        ClientPlatform.AndroidPhoneArm64 |
        ClientPlatform.AndroidPhoneArm |
        ClientPlatform.iOSArm64;

    /// <summary>
    /// 平板电脑
    /// </summary>
    public const ClientPlatform DeviceIdiom_Tablet =
        ClientPlatform.iPadOSArm64 |
        ClientPlatform.AndroidPadX64 |
        ClientPlatform.AndroidPadX86 |
        ClientPlatform.AndroidPadArm64 |
        ClientPlatform.AndroidPadArm;

    /// <summary>
    /// 桌面
    /// </summary>
    public const ClientPlatform DeviceIdiom_Desktop =
        ClientPlatform.Win32X86 |
        ClientPlatform.Win32X64 |
        ClientPlatform.Win32Arm64 |
        ClientPlatform.Win32StoreX86 |
        ClientPlatform.Win32StoreX64 |
        ClientPlatform.Win32StoreArm64 |
        ClientPlatform.UWPX86 |
        ClientPlatform.UWPX64 |
        ClientPlatform.UWPArm64 |
        ClientPlatform.macOSX64 |
        ClientPlatform.macOSArm64 |
        ClientPlatform.LinuxX64 |
        ClientPlatform.LinuxArm64 |
        ClientPlatform.LinuxArm |
        ClientPlatform.LinuxLoongArch64;

    /// <summary>
    /// 电视
    /// </summary>
    public const ClientPlatform DeviceIdiom_TV =
        ClientPlatform.tvOSArm64 |
        ClientPlatform.AndroidTVX64 |
        ClientPlatform.AndroidTVX86 |
        ClientPlatform.AndroidTVArm64 |
        ClientPlatform.AndroidTVArm;

    /// <summary>
    /// 手表
    /// </summary>
    public const ClientPlatform DeviceIdiom_Watch =
        ClientPlatform.watchOSArm64 |
        ClientPlatform.AndroidWearArm64;

    #endregion DeviceIdiom

    /// <summary>
    /// 将【客户端平台枚举】转换为【WebApi 定义的平台枚举】
    /// </summary>
    [Obsolete(
    """
use DevicePlatform2 Or ClientPlatform replace on top of newly added content.
oldType: System.Runtime.Devices.Platform
""")]
    public static WebApiCompatDevicePlatform ToPlatform(this ClientPlatform clientPlatform)
    {
        WebApiCompatDevicePlatform result = default;
        foreach (var item in EnumFlagsSplit(clientPlatform))
        {
            if (Platform_Windows.HasFlag(item)) result |= WebApiCompatDevicePlatform.Windows;
            if (Platform_Linux.HasFlag(item)) result |= WebApiCompatDevicePlatform.Linux;
            if (Platform_Android.HasFlag(item)) result |= WebApiCompatDevicePlatform.Android;
            if (Platform_Apple.HasFlag(item)) result |= WebApiCompatDevicePlatform.Apple;
            if (Platform_UWP.HasFlag(item)) result |= WebApiCompatDevicePlatform.UWP;
        }
        return result != default ? result : WebApiCompatDevicePlatform.Unknown;
    }

    /// <summary>
    /// 将【客户端平台枚举】转换【CPU 架构的 Flags 枚举】
    /// </summary>
    public static ArchitectureFlags ToArchitectureFlags(this ClientPlatform clientPlatform)
    {
        ArchitectureFlags result = default;
        foreach (var item in EnumFlagsSplit(clientPlatform))
        {
            if (ArchitectureFlags_X86.HasFlag(item)) result |= ArchitectureFlags.X86;
            if (ArchitectureFlags_Arm.HasFlag(item)) result |= ArchitectureFlags.Arm;
            if (ArchitectureFlags_X64.HasFlag(item)) result |= ArchitectureFlags.X64;
            if (ArchitectureFlags_Arm64.HasFlag(item)) result |= ArchitectureFlags.Arm64;
            if (ArchitectureFlags_LoongArch64.HasFlag(item)) result |= ArchitectureFlags.LoongArch64;
        }
        return result != default ? result : default;
    }

    /// <summary>
    /// 将【客户端平台枚举】转换为【设备种类枚举】
    /// </summary>
    public static DeviceIdiom ToDeviceIdiom(this ClientPlatform clientPlatform)
    {
        DeviceIdiom result = default;
        foreach (var item in EnumFlagsSplit(clientPlatform))
        {
            if (DeviceIdiom_Phone.HasFlag(item)) result |= DeviceIdiom.Phone;
            if (DeviceIdiom_Tablet.HasFlag(item)) result |= DeviceIdiom.Tablet;
            if (DeviceIdiom_Desktop.HasFlag(item)) result |= DeviceIdiom.Desktop;
            if (DeviceIdiom_TV.HasFlag(item)) result |= DeviceIdiom.TV;
            if (DeviceIdiom_Watch.HasFlag(item)) result |= DeviceIdiom.Watch;
        }
        return result != default ? result : DeviceIdiom.Unknown;
    }

    /// <summary>
    /// 将【CPU 架构的 Flags 枚举】转换为【客户端平台枚举】
    /// </summary>
    public static ClientPlatform ToClientPlatform(this ArchitectureFlags archFlags)
    {
        ClientPlatform result = default;
        foreach (var item in EnumFlagsSplit(archFlags))
        {
            result |= item switch
            {
                ArchitectureFlags.Arm64 => ArchitectureFlags_Arm64,
                ArchitectureFlags.X86 => ArchitectureFlags_X86,
                ArchitectureFlags.Arm => ArchitectureFlags_Arm,
                ArchitectureFlags.X64 => ArchitectureFlags_X64,
                ArchitectureFlags.LoongArch64 => ArchitectureFlags_LoongArch64,
                _ => default,
            };
        }
        return result;
    }

    /// <summary>
    /// 将【WebApi 定义的平台枚举】转换为【客户端平台枚举】
    /// </summary>
    [Obsolete(
"""
use DevicePlatform2 Or ClientPlatform replace on top of newly added content.
oldType: System.Runtime.Devices.Platform
""")]
    public static ClientPlatform ToClientPlatform(this WebApiCompatDevicePlatform devicePlatform)
    {
        ClientPlatform result = default;
        foreach (var item in EnumFlagsSplit(devicePlatform))
        {
            result |= item switch
            {
                WebApiCompatDevicePlatform.Windows => Platform_Windows,
                WebApiCompatDevicePlatform.Linux => Platform_Linux,
                WebApiCompatDevicePlatform.Android => Platform_Android,
                WebApiCompatDevicePlatform.Apple => Platform_Apple,
                WebApiCompatDevicePlatform.UWP => Platform_UWP,
                _ => default,
            };
        }
        return result;
    }

    /// <summary>
    /// 将【设备种类枚举】转换为【客户端平台枚举】
    /// </summary>
    public static ClientPlatform ToClientPlatform(this DeviceIdiom deviceIdiom)
    {
        ClientPlatform result = default;
        foreach (var item in EnumFlagsSplit(deviceIdiom))
        {
            result |= item switch
            {
                DeviceIdiom.TV => DeviceIdiom_TV,
                DeviceIdiom.Phone => DeviceIdiom_Phone,
                DeviceIdiom.Tablet => DeviceIdiom_Tablet,
                DeviceIdiom.Desktop => DeviceIdiom_Desktop,
                DeviceIdiom.Watch => DeviceIdiom_Watch,
                _ => default,
            };
        }
        return result;
    }

    /// <summary>
    /// 将 WebApiCompatDevicePlatform、DeviceIdiom、ArchitectureFlags 转换为 ClientPlatform
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="deviceIdiom"></param>
    /// <param name="architecture"></param>
    /// <param name="distinguishMsStore">区分到微软商店</param>
    /// <returns></returns>
    [Obsolete(
"""
use DevicePlatform2 Or ClientPlatform replace on top of newly added content.
oldType: System.Runtime.Devices.Platform
""")]
    public static ClientPlatform ToClientPlatform(
        WebApiCompatDevicePlatform platform,
        DeviceIdiom deviceIdiom,
        ArchitectureFlags architecture,
        bool? distinguishMsStore = null)
    {
        var clientPlatform =
            platform.ToClientPlatform() &
            deviceIdiom.ToClientPlatform() &
            architecture.ToClientPlatform();

        if (distinguishMsStore.HasValue)
        {
            clientPlatform &= distinguishMsStore.Value
                // 仅保留微软商店的
                ? Platform_Windows_OnlyStore
                // 仅移除微软商店的
                : ~Platform_Windows_OnlyStore;
        }

        return clientPlatform;
    }

    /// <summary>
    /// 将【设备平台枚举】与【CPU 架构的枚举】转换为【客户端平台枚举】
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ClientPlatform GetClientPlatform(this DevicePlatform2 platform2, Architecture architecture, bool isPublishToStore = false, DeviceIdiom deviceIdiom = DeviceIdiom.Desktop, bool @throw = true)
    {
        switch (platform2)
        {
            case DevicePlatform2.Windows:
            case DevicePlatform2.WinUI:
            case DevicePlatform2.UWP:
                switch (architecture)
                {
                    case Architecture.X86:
                        return isPublishToStore ?
                            ClientPlatform.Win32StoreX86 : ClientPlatform.Win32X86;
                    case Architecture.X64:
                        return isPublishToStore ?
                            ClientPlatform.Win32StoreX64 : ClientPlatform.Win32X64;
                    case Architecture.Arm64:
                        return isPublishToStore ?
                            ClientPlatform.Win32StoreArm64 : ClientPlatform.Win32Arm64;
                }
                break;
            case DevicePlatform2.WSA:
            case DevicePlatform2.AndroidUnknown:
            case DevicePlatform2.AndroidPhone:
            case DevicePlatform2.AndroidTablet:
            case DevicePlatform2.AndroidDesktop:
            case DevicePlatform2.AndroidTV:
            case DevicePlatform2.AndroidWatch:
            case DevicePlatform2.AndroidVirtual:
            case DevicePlatform2.ChromeOS:
                switch (architecture)
                {
                    case Architecture.X86:
                        return ClientPlatform.AndroidPadX86;
                    case Architecture.X64:
                        return ClientPlatform.AndroidPhoneX64;
                    case Architecture.Arm64:
                        return ClientPlatform.AndroidPhoneArm64;
                    case Architecture.Arm:
                        return ClientPlatform.AndroidPhoneArm;
                }
                break;
            case DevicePlatform2.iOS:
            case DevicePlatform2.iPadOS:
            case DevicePlatform2.macOS:
            case DevicePlatform2.MacCatalyst:
            case DevicePlatform2.watchOS:
            case DevicePlatform2.tvOS:
                switch (deviceIdiom)
                {
                    case DeviceIdiom.Desktop:
                        switch (architecture)
                        {
                            case Architecture.X64:
                                return ClientPlatform.macOSX64;
                            case Architecture.Arm64:
                                return ClientPlatform.macOSArm64;
                        }
                        break;
                    case DeviceIdiom.Phone:
                        return ClientPlatform.iOSArm64;
                    case DeviceIdiom.Tablet:
                        return ClientPlatform.iPadOSArm64;
                    case DeviceIdiom.TV:
                        return ClientPlatform.tvOSArm64;
                    case DeviceIdiom.Watch:
                        return ClientPlatform.watchOSArm64;
                }
                break;
            case DevicePlatform2.Linux:
                switch (architecture)
                {
                    case Architecture.X64:
                        return ClientPlatform.LinuxX64;
                    case Architecture.Arm64:
                        return ClientPlatform.LinuxArm64;
                    case Architecture.Arm:
                        return ClientPlatform.LinuxArm;
                }
                break;
        }
        return default;
    }
}