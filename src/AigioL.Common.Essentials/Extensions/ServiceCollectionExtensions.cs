using AigioL.Common.Essentials.ApplicationModel;
using AigioL.Common.Essentials.ApplicationModel.Implementation;
using AigioL.Common.Essentials.Devices;
using AigioL.Common.Essentials.Devices.Implementation;
using AigioL.Common.Essentials.Storage;
using AigioL.Common.Essentials.Storage.Implementation;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddEssential(
        this IServiceCollection services,
        string packageName,
        string? keySecureStorage,
        string versionString,
        string buildString,
        string appDataDirectory,
        bool isSecureStorageCurrentUserOrLocalMachine = false)
    {
        var preferences = new UnpackagedPreferencesImplementation(appDataDirectory);
        var secureStorage = new UnpackagedSecureStorageImplementation(
            preferences,
            keySecureStorage,
            packageName,
            appDataDirectory,
            isSecureStorageCurrentUserOrLocalMachine);
        var versionTracking = new VersionTrackingImplementation(
            preferences,
            packageName,
            versionString,
            buildString);

        services.AddSingleton<IPreferences>(preferences);
        services.AddSingleton<ISecureStorage>(secureStorage);
        services.AddSingleton<IVersionTracking>(versionTracking);
        services.TryAddSingleton<IDeviceInfo, DeviceInfoImplementation>();
        return services;
    }
}
