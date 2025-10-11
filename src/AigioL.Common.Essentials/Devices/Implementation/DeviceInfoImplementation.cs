#if WINDOWS
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
#elif ANDROID
using Android.App;
using Android.Content.Res;
using Android.OS;
using Android.Provider;
#elif MACOS
using Foundation;
using ObjCRuntime;
#elif __WATCHOS__
using WatchKit;
using UIDevice = WatchKit.WKInterfaceDevice;
using ObjCRuntime;
#elif MACCATALYST || __TVOS__
using UIKit;
using ObjCRuntime;
#endif
using AigioL.Common.Primitives.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AigioL.Common.Essentials.Devices.Implementation;

public partial class DeviceInfoImplementation : IDeviceInfo
{
    public DeviceInfoImplementation()
    {
#if WINDOWS
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240))
        {
            deviceInfo = new EasClientDeviceInformation();
            try
            {
                systemProductName = deviceInfo.SystemProductName;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get system product name. {ex.Message}");
            }
        }
#endif
    }

    public virtual DeviceType DeviceType
    {
        get
        {
#if WINDOWS
            if (currentType != DeviceType.Unknown)
                return currentType;

            try
            {
                if (string.IsNullOrWhiteSpace(systemProductName))
                    systemProductName = deviceInfo?.SystemProductName;

                var isVirtual = systemProductName == null ||
                    systemProductName.Contains("Virtual", StringComparison.Ordinal) || systemProductName == "HMV domU";

                currentType = isVirtual ? DeviceType.Virtual : DeviceType.Physical;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get device type. {ex.Message}");
            }
            return currentType;
#elif ANDROID
            var isEmulator =
            	(Build.Brand.StartsWith("generic", StringComparison.Ordinal) && Build.Device.StartsWith("generic", StringComparison.Ordinal)) ||
            	Build.Fingerprint.StartsWith("generic", StringComparison.Ordinal) ||
            	Build.Fingerprint.StartsWith("unknown", StringComparison.Ordinal) ||
            	Build.Hardware.Contains("goldfish", StringComparison.Ordinal) ||
            	Build.Hardware.Contains("ranchu", StringComparison.Ordinal) ||
            	Build.Model.Contains("google_sdk", StringComparison.Ordinal) ||
            	Build.Model.Contains("Emulator", StringComparison.Ordinal) ||
            	Build.Model.Contains("Android SDK built for x86", StringComparison.Ordinal) ||
            	Build.Manufacturer.Contains("Genymotion", StringComparison.Ordinal) ||
            	Build.Manufacturer.Contains("VS Emulator", StringComparison.Ordinal) ||
            	Build.Product.Contains("emulator", StringComparison.Ordinal) ||
            	Build.Product.Contains("google_sdk", StringComparison.Ordinal) ||
            	Build.Product.Contains("sdk", StringComparison.Ordinal) ||
            	Build.Product.Contains("sdk_google", StringComparison.Ordinal) ||
            	Build.Product.Contains("sdk_x86", StringComparison.Ordinal) ||
            	Build.Product.Contains("simulator", StringComparison.Ordinal) ||
            	Build.Product.Contains("vbox86p", StringComparison.Ordinal);
            
            if (isEmulator)
            	return DeviceType.Virtual;
            
            return DeviceType.Physical;
#elif MACCATALYST || MACOS
            return DeviceType.Physical;
			DeviceType.Physical;
#elif __TVOS__ || __WATCHOS__
			Runtime.Arch == Arch.DEVICE ? DeviceType.Physical : DeviceType.Virtual;
#else
            return DeviceType.Unknown;
#endif
        }
    }

    public virtual string Model
    {
        get
        {
#if WINDOWS
            return deviceInfo?.SystemProductName ?? string.Empty;
#elif ANDROID
            return Build.Model;
#elif MACOS
            return IOKit.GetPlatformExpertPropertyValue<NSData>("model")?.ToString() ?? string.Empty;
#elif MACCATALYST || __TVOS__ || __WATCHOS__ || IOS
            try
            {
            	return PlatformUtils.GetSystemLibraryProperty("hw.machine");
            }
            catch (Exception)
            {
            	Debug.WriteLine("Unable to query hardware model, returning current device model.");
            }
            return UIDevice.CurrentDevice.Model;
#else
            return string.Empty;
#endif
        }
    }

    public virtual string Manufacturer
    {
        get
        {
#if WINDOWS
            return deviceInfo?.SystemManufacturer ?? string.Empty;
#elif ANDROID
            return Build.Manufacturer;
#elif MACOS || MACCATALYST || __TVOS__ || __WATCHOS__ || IOS
            return "Apple";
#else
            return string.Empty;
#endif
        }
    }

    public virtual string Name
    {
        get
        {
#if WINDOWS
            return deviceInfo?.FriendlyName ?? string.Empty;
#elif ANDROID
            // DEVICE_NAME added in System.Global in API level 25
            // https://developer.android.com/reference/android/provider/Settings.Global#DEVICE_NAME
            var name = GetSystemSetting("device_name", true);
            if (string.IsNullOrWhiteSpace(name))
            	name = Model;
            return name;
#elif MACOS
            var computerNameHandle = SCDynamicStoreCopyComputerName(IntPtr.Zero, IntPtr.Zero);
            
            if (computerNameHandle == IntPtr.Zero)
            	return string.Empty;
            
            try
            {
#pragma warning disable CS0618 // Type or member is obsolete
                return NSString.FromHandle(computerNameHandle);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            finally
            {
            	CFRelease(computerNameHandle);
            }
#elif MACCATALYST || __TVOS__ || __WATCHOS__ || IOS
            return UIDevice.CurrentDevice.Name;
#else
            return string.Empty;
#endif
        }
    }

    public virtual DeviceIdiom Idiom
    {
        get
        {
#if WINDOWS
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240))
            {
                currentIdiom = AnalyticsInfo.VersionInfo.DeviceFamily switch
                {
                    "Windows.Mobile" => DeviceIdiom.Phone,
                    "Windows.Universal" or "Windows.Desktop" => GetIsInTabletMode()
                                            ? DeviceIdiom.Tablet
                                            : DeviceIdiom.Desktop,
                    "Windows.Xbox" or "Windows.Team" => DeviceIdiom.TV,
                    _ => DeviceIdiom.Unknown,
                };
                return currentIdiom;
            }
            else
            {
                return currentIdiom = DeviceIdiom.Desktop;
            }
#elif ANDROID
            var currentIdiom = DeviceIdiom.Unknown;

            // first try UIModeManager
            var uiModeManager = UiModeManager.FromContext(Application.Context);
            
            try
            {
            	var uiMode = uiModeManager?.CurrentModeType ?? UiMode.TypeUndefined;
            	currentIdiom = DetectIdiom(uiMode);
            }
            catch (Exception ex)
            {
            	System.Diagnostics.Debug.WriteLine($"Unable to detect using UiModeManager: {ex.Message}");
            }
            
            // then try Configuration
            if (currentIdiom == DeviceIdiom.Unknown)
            {
            	var configuration = Application.Context.Resources?.Configuration;
            	if (configuration != null)
            	{
            		var minWidth = configuration.SmallestScreenWidthDp;
            		var isWide = minWidth >= tabletCrossover;
            		currentIdiom = isWide ? DeviceIdiom.Tablet : DeviceIdiom.Phone;
            	}
            	else
            	{
            		// start clutching at straws
            		var metrics = Application.Context.Resources?.DisplayMetrics;
            		if (metrics != null)
            		{
            			var minSize = Math.Min(metrics.WidthPixels, metrics.HeightPixels);
            			var isWide = minSize * metrics.Density >= tabletCrossover;
            			currentIdiom = isWide ? DeviceIdiom.Tablet : DeviceIdiom.Phone;
            		}
            	}
            }
            
            // hope we got it somewhere
            return currentIdiom;
#elif MACOS
            return DeviceIdiom.Desktop;
#elif __WATCHOS__
			return DeviceIdiom.Watch;
#elif __TVOS__
			return DeviceIdiom.TV;
#elif MACCATALYST || IOS
			switch (UIDevice.CurrentDevice.UserInterfaceIdiom)
			{
				case UIUserInterfaceIdiom.Pad:
					return DeviceIdiom.Tablet;
				case UIUserInterfaceIdiom.Phone:
					return DeviceIdiom.Phone;
				case UIUserInterfaceIdiom.TV:
					return DeviceIdiom.TV;
				case UIUserInterfaceIdiom.CarPlay:
				case UIUserInterfaceIdiom.Unspecified:
				default:
					return DeviceIdiom.Unknown;
			}
#else
            return DeviceIdiom.Unknown;
#endif
        }
    }

    public virtual DevicePlatform2 Platform => Utils.Platform;

    public virtual Version Version => Utils.ParseVersion(VersionString);

    public virtual string VersionString
    {
        get
        {
#if ANDROID
            return Build.VERSION.Release;
#elif MACOS
            using var info = new NSProcessInfo();
            return info.OperatingSystemVersion.ToString();
#elif MACCATALYST || __TVOS__ || __WATCHOS__ || IOS
            return UIDevice.CurrentDevice.SystemVersion;
#else
            return Environment.OSVersion.VersionString;
#endif
        }
    }
}

#if WINDOWS
partial class DeviceInfoImplementation // https://github.com/dotnet/maui/blob/10.0.0-rc.1.25424.2/src/Essentials/src/DeviceInfo/DeviceInfo.windows.cs
{
    protected readonly EasClientDeviceInformation? deviceInfo;
    protected DeviceIdiom currentIdiom;
    protected DeviceType currentType = DeviceType.Unknown;
    protected string? systemProductName;

    /// <summary>
    /// Whether or not the device is in "tablet mode" or not. This
    /// has to be implemented by the device manufacturer.
    /// </summary>
    const int SM_CONVERTIBLESLATEMODE = 0x2003;

    /// <summary>
    /// How many fingers (aka touches) are supported for touch control
    /// </summary>
    const int SM_MAXIMUMTOUCHES = 95;

    /// <summary>
    /// Whether a physical keyboard is attached or not.
    /// Manufacturers have to remember to set this.
    /// Defaults to not-attached.
    /// </summary>
    const int SM_ISDOCKED = 0x2004;


    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial int GetSystemMetrics(int nIndex);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetAutoRotationState(ref AutoRotationState pState);

    [LibraryImport("Powrprof.dll", SetLastError = true)]
    private static partial PowerPlatformRole PowerDeterminePlatformRoleEx(ulong Version);

    static bool GetIsInTabletMode()
    {
        // Adopt Chromium's methodology for determining tablet mode
        // https://source.chromium.org/chromium/chromium/src/+/main:base/win/win_util.cc;l=537;drc=ac83a5a2d3c04763d86ce16d92f3904cc9566d3a;bpv=0;bpt=1
        // Device does not have a touchscreen
        if (GetSystemMetrics(SM_MAXIMUMTOUCHES) == 0)
        {
            return false;
        }

        // If the device is docked, user is treating as a PC
        if (GetSystemMetrics(SM_ISDOCKED) != 0)
        {
            return false;
        }

        // Fetch device rotation. Possible for this to fail.
        AutoRotationState rotationState = AutoRotationState.AR_ENABLED;
        bool success = GetAutoRotationState(ref rotationState);

        // Fetch succeeded and device does not support rotation
        if (success && (rotationState & (AutoRotationState.AR_NOT_SUPPORTED | AutoRotationState.AR_LAPTOP | AutoRotationState.AR_NOSENSOR)) != 0)
        {
            return false;
        }

        // Check if power management says we are mobile (laptop) or a tablet
        if ((PowerDeterminePlatformRoleEx(2) & (PowerPlatformRole.PlatformRoleMobile | PowerPlatformRole.PlatformRoleSlate)) != 0)
        {
            // Check if tablet mode is 0. 0 is default value.
            return GetSystemMetrics(SM_CONVERTIBLESLATEMODE) == 0;
        }

        return false;
    }

    /// <summary>
    /// Represents the OEM's preferred power management profile,
    /// Useful in-case OEM implements one but not the other
    /// </summary>
    enum PowerPlatformRole
    {
        PlatformRoleMobile = 2,
        PlatformRoleSlate = 8,
    }

    /// <summary>
    /// Whether rotation is supported or not.
    /// Rotation is only supported if AR_ENABLED is true
    /// </summary>
    enum AutoRotationState
    {
        AR_ENABLED = 0x0,
        AR_NOT_SUPPORTED = 0x20,
        AR_LAPTOP = 0x80,
        AR_NOSENSOR = 0x10,
    }
}
#elif ANDROID
partial class DeviceInfoImplementation // https://github.com/dotnet/maui/blob/10.0.0-rc.1.25424.2/src/Essentials/src/DeviceInfo/DeviceInfo.android.cs
{
        static DeviceIdiom DetectIdiom(UiMode uiMode)
		{
			if (uiMode == UiMode.TypeNormal)
				return DeviceIdiom.Unknown;
			else if (uiMode == UiMode.TypeTelevision)
				return DeviceIdiom.TV;
			else if (uiMode == UiMode.TypeDesk)
				return DeviceIdiom.Desktop;
			else if (uiMode == UiMode.TypeWatch)
				return DeviceIdiom.Watch;

			return DeviceIdiom.Unknown;
		}

        static string GetSystemSetting(string name, bool isGlobal = false)
		{
			if (isGlobal && OperatingSystem.IsAndroidVersionAtLeast(25))
				return Settings.Global.GetString(Application.Context.ContentResolver, name);
			else
				return Settings.System.GetString(Application.Context.ContentResolver, name);
		}
}
#elif MACOS
partial class DeviceInfoImplementation // https://github.com/dotnet/maui/blob/10.0.0-rc.1.25424.2/src/Essentials/src/DeviceInfo/DeviceInfo.macos.cs
{
        [DllImport(Constants.SystemConfigurationLibrary)]
        static extern IntPtr SCDynamicStoreCopyComputerName(IntPtr store, IntPtr encoding);

        [DllImport(Constants.CoreFoundationLibrary)]
        static extern void CFRelease(IntPtr cf);
}
#endif

file static class Utils
{
    /// <summary>
    /// https://github.com/dotnet/maui/blob/10.0.0-rc.1.25424.2/src/Essentials/src/Types/Shared/Utils.shared.cs#L10
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    internal static Version ParseVersion(string version)
    {
        if (Version.TryParse(version, out var number))
            return number;

        if (int.TryParse(version, out var major))
            return new Version(major, 0);

        return new Version(0, 0);
    }

    static DevicePlatform2? platform;

    internal static DevicePlatform2 Platform => platform ??= GetPlatform();

    static DevicePlatform2 GetPlatform()
    {
#if WINDOWS
        return DevicePlatform2.Windows;
#elif ANDROID
        return DevicePlatform2.AndroidPhone;
#elif IOS
        return DevicePlatform2.iOS;
#elif MACOS
        return DevicePlatform2.macOS;
#elif __WATCHOS__
        return DevicePlatform2.watchOS;
#elif __TVOS__
        return DevicePlatform2.tvOS;
#elif MACCATALYST
        if (OperatingSystem.IsIOS())
        {
            return DevicePlatform2.iOS;
        }
        else
        {
            return DevicePlatform2.macOS;
        }
#elif LINUX
        return DevicePlatform2.Linux;
#else
        if (OperatingSystem.IsWindows())
        {
            return DevicePlatform2.Windows;
        }
        else if (OperatingSystem.IsAndroid())
        {
            return DevicePlatform2.AndroidPhone;
        }
        else if (OperatingSystem.IsIOS())
        {
            return DevicePlatform2.iOS;
        }
        else if (OperatingSystem.IsMacOS())
        {
            return DevicePlatform2.macOS;
        }
        else if (OperatingSystem.IsTvOS())
        {
            return DevicePlatform2.tvOS;
        }
        else if (OperatingSystem.IsWatchOS())
        {
            return DevicePlatform2.watchOS;
        }
        else if (OperatingSystem.IsLinux())
        {
            return DevicePlatform2.Linux;
        }
        return default;
#endif
    }

}

#if MACCATALYST || __TVOS__ || __WATCHOS__ || IOS
file static class PlatformUtils // https://github.com/dotnet/maui/blob/10.0.0-rc.1.25424.2/src/Essentials/src/Platform/PlatformUtils.ios.tvos.watchos.cs
{
#if IOS
    [DllImport(Constants.SystemLibrary, EntryPoint = "sysctlbyname")]
#elif MACCATALYST || __TVOS__ || __WATCHOS__
    [DllImport(Constants.libSystemLibrary, EntryPoint = "sysctlbyname")]
    internal static extern int SysctlByName([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);
#endif

    internal static string GetSystemLibraryProperty(string property)
    {
        var lengthPtr = Marshal.AllocHGlobal(sizeof(int));
        SysctlByName(property, IntPtr.Zero, lengthPtr, IntPtr.Zero, 0);

        var propertyLength = Marshal.ReadInt32(lengthPtr);

        if (propertyLength == 0)
        {
            Marshal.FreeHGlobal(lengthPtr);
            throw new InvalidOperationException("Unable to read length of property.");
        }

        var valuePtr = Marshal.AllocHGlobal(propertyLength);
        SysctlByName(property, valuePtr, lengthPtr, IntPtr.Zero, 0);

        var returnValue = Marshal.PtrToStringAnsi(valuePtr);

        Marshal.FreeHGlobal(lengthPtr);
        Marshal.FreeHGlobal(valuePtr);

        return returnValue;
    }
}
#endif