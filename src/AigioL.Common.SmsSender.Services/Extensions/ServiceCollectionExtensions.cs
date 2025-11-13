using AigioL.Common.SmsSender.Models;
using AigioL.Common.SmsSender.Models.Abstractions;
using AigioL.Common.SmsSender.Services;
using AigioL.Common.SmsSender.Services.Implementation.SmsSender;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;
using CHANNELS = AigioL.Common.SmsSender.Services.Implementation.SmsSender.Channels;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加短信发送提供商(仅单选一个提供商)
    /// </summary>
    /// <typeparam name="TSmsSettings"></typeparam>
    /// <param name="services"></param>
    /// <param name="settings"></param>
    /// <param name="name">提供商唯一名称，见 <see cref="ISmsSettings.SmsOptions"/> 中 PropertyName</param>
    /// <returns></returns>
    public static IServiceCollection AddSmsSenderProvider<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TSmsSettings>(
        this IServiceCollection services,
        TSmsSettings settings,
        string? name = null)
        where TSmsSettings : class, ISmsSettings
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            name = SmsOptions.GetDefaultProviderName(settings.SmsOptions);
        }
        return name switch
        {
            null => services.AddDebugSmsSenderProvider(),
            nameof(ISmsSettings.SmsOptions._21VianetBlueCloud)
                => Add<CHANNELS._21VianetBlueCloud.SenderProviderInvoker<TSmsSettings>, TSmsSettings>(services),
            nameof(ISmsSettings.SmsOptions.AlibabaCloud)
                => Add<CHANNELS.AlibabaCloud.SenderProviderInvoker<TSmsSettings>, TSmsSettings>(services),
            nameof(ISmsSettings.SmsOptions.NetEaseCloud)
                => Add<CHANNELS.NetEaseCloud.SenderProviderInvoker<TSmsSettings>, TSmsSettings>(services),
            nameof(ISmsSettings.SmsOptions.HuaweiCloud)
                => Add<CHANNELS.HuaweiCloud.SenderProviderInvoker<TSmsSettings>, TSmsSettings>(services),
            nameof(ISmsSettings.SmsOptions.TencentCloud)
                => Add<CHANNELS.TencentCloud.SenderProviderInvoker<TSmsSettings>, TSmsSettings>(services),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null),
        };
    }

    static IServiceCollection AddDebugSmsSenderProvider(this IServiceCollection services)
    {
        services.AddScoped<DebugSmsSenderProvider>();
        services.AddScoped<ISmsSender>(static s => s.GetRequiredService<DebugSmsSenderProvider>());
        return services;
    }

    static IServiceCollection Add<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSmsSender,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TSmsSettings>(
        IServiceCollection services)
        where TSmsSender : class, ISmsSender
        where TSmsSettings : class, ISmsSettings
    {
        services.AddHttpClient<TSmsSender>();
        services.TryAddScoped<DebugSmsSenderProvider>();
        services.AddScoped<ISmsSender, SmsSenderWrapper<TSmsSender, TSmsSettings>>();
        return services;
    }
}
