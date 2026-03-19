using AigioL.Common.Essentials;
using AigioL.Common.Essentials.ApplicationModel;
using AigioL.Common.Essentials.Devices;
using AigioL.Common.Essentials.Devices.Implementation;
using AigioL.Common.Essentials.Storage;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddEssential(this IServiceCollection services)
    {
        services.AddSingleton<IPreferences>(EssentialInit.Preferences);
        services.AddSingleton<ISecureStorage>(EssentialInit.SecureStorage);
        services.AddSingleton<IVersionTracking>(EssentialInit.VersionTracking);
        services.AddSingleton<IDeviceInfo>(EssentialInit.DeviceInfo);
        return services;
    }
}