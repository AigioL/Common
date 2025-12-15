using AigioL.Common.Storage.Models;
using AigioL.Common.Storage.Models.Abstractions;
using AigioL.Common.Storage.Services;
using AigioL.Common.Storage.Services.Implementation.TencentCloud;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddObjectStorageService<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TSettings>(
        this IServiceCollection services,
        TSettings settings,
        string? name = null)
        where TSettings : class, IObjectStorageSettings
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            name = ObjectStorageOptions.GetDefaultProviderName(settings.ObjectStorageOptions);
        }
        return name switch
        {
            null => services,
            nameof(ObjectStorageOptions.TencentCloud)
                => Add<TencentCloudStorageService<TSettings>>(services),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null),
        };
    }

    static IServiceCollection Add<
       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImpl>(
       IServiceCollection services)
       where TImpl : class, IObjectStorageService
    {
        services.TryAddScoped<IObjectStorageService, TImpl>();
        return services;
    }
}
