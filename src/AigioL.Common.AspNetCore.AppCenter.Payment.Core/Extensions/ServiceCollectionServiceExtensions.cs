using AigioL.Common.AspNetCore.AppCenter.Payment.Models;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionServiceExtensions
{
    /// <summary>
    /// 添加支付相关服务
    /// </summary>
    [RequiresDynamicCode("Binding strongly typed objects to configuration values may require generating dynamic code at runtime.")]
    [RequiresUnreferencedCode("TOptions's dependent types may have their members trimmed. Ensure all required members are preserved.")]
    public static IServiceCollection AddPaymentServices<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        this IHostApplicationBuilder b)
        where TAppSettings : class, IPaySettings
    {
        var services = b.Services;
        var cfg = b.Configuration;

        services.TryAddScoped<IPaymentMessageQueueService, PaymentMessageQueueService>();

        services.Configure<AlipayExOptions>(cfg.GetSection("AppSettings:PayOptions:AliPay"));
        services.Configure<AlipayExOptions>("Mini", cfg.GetSection("AppSettings:PayOptions:AliPayMini"));
        services.AddAlipay();
        services.TryAddScoped<IAliPayServices, AliPayServices<TAppSettings>>();

        services.Configure<WeChatPayExOptions>(cfg.GetSection("AppSettings:PayOptions:WechatPay"));
        services.AddWeChatPay();
        services.TryAddScoped<IWeChatPayServices, WeChatPayV3Services<TAppSettings>>();

        services.TryAddScoped<IPaymentService, PaymentService>();
        return services;
    }
}
