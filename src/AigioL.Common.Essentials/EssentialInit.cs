using AigioL.Common.Essentials.ApplicationModel;
using AigioL.Common.Essentials.ApplicationModel.Implementation;
using AigioL.Common.Essentials.Devices;
using AigioL.Common.Essentials.Devices.Implementation;
using AigioL.Common.Essentials.Storage;
using AigioL.Common.Essentials.Storage.Implementation;

namespace AigioL.Common.Essentials;

public static partial class EssentialInit
{
    public static void InitEssential(
        string packageName,
        string? keySecureStorage,
        string versionString,
        string buildString,
        string appDataDirectory,
        bool isSecureStorageCurrentUserOrLocalMachine = false,
        IDeviceInfo? deviceInfo = null)
    {
        Preferences = new UnpackagedPreferencesImplementation(appDataDirectory);
        SecureStorage = new UnpackagedSecureStorageImplementation(
            Preferences,
            keySecureStorage,
            packageName,
            appDataDirectory,
            isSecureStorageCurrentUserOrLocalMachine);
        VersionTracking = new VersionTrackingImplementation(
            Preferences,
            packageName,
            versionString,
            buildString);
        DeviceInfo = deviceInfo ?? new DeviceInfoImplementation();
    }
}

static partial class EssentialInit
{
    internal static IPreferences Preferences
    {
        get
        {
            var preferences = field;
            ArgumentNullException.ThrowIfNull(preferences);
            return preferences;
        }
        private set => field = value;
    }

    internal static ISecureStorage SecureStorage
    {
        get
        {
            var secureStorage = field;
            ArgumentNullException.ThrowIfNull(secureStorage);
            return secureStorage;
        }
        private set => field = value;
    }

    internal static IVersionTracking VersionTracking
    {
        get
        {
            var versionTracking = field;
            ArgumentNullException.ThrowIfNull(versionTracking);
            return versionTracking;
        }
        private set => field = value;
    }

    internal static IDeviceInfo DeviceInfo
    {
        get
        {
            var deviceInfo = field;
            ArgumentNullException.ThrowIfNull(deviceInfo);
            return deviceInfo;
        }
        private set => field = value;
    }
}
