using AigioL.Essentials.ApplicationModel;
using AigioL.Essentials.ApplicationModel.Implementation;
using AigioL.Essentials.Storage;
using AigioL.Essentials.Storage.Implementation;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddEssentialStorage(
        this IServiceCollection services,
        string packageName,
        string versionString,
        string buildString,
        string appDataDirectory,
        bool isCurrentUserOrLocalMachine = false)
    {
        var preferences = new UnpackagedPreferencesImplementation(appDataDirectory);
        var secureStorage = new UnpackagedSecureStorageImplementation(
            preferences,
            packageName,
            appDataDirectory,
            isCurrentUserOrLocalMachine);
        var versionTracking = new VersionTrackingImplementation(
            preferences,
            packageName,
            versionString,
            buildString);

        services.AddSingleton<IPreferences>(preferences);
        services.AddSingleton<ISecureStorage>(secureStorage);
        services.AddSingleton<IVersionTracking>(versionTracking);
        return services;
    }
}
