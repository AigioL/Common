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