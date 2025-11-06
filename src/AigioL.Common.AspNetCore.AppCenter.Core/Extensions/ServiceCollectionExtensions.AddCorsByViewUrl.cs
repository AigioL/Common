using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 配置允许跨域访问的 Web UI 地址
    /// </summary>
    /// <param name="services"></param>
    /// <param name="appSettings"></param>
    public static void AddCorsByViewUrl(this IServiceCollection services, IViewsUrl appSettings)
    {
        if (!string.IsNullOrWhiteSpace(appSettings.ViewsUrl))
        {
            var origins = appSettings.GetOrigins();
            if (origins.Length != 0)
            {
                services.AddCors(options =>
                {
                    options.AddDefaultPolicy(
                        builder => builder.WithOrigins(origins).AllowCredentials().AllowAnyMethod().AllowAnyHeader());
                });
            }
        }
    }

    /// <summary>
    /// 配置允许跨域的中间件
    /// </summary>
    public static IApplicationBuilder UseCors<TAppSettings>(this IApplicationBuilder builder, TAppSettings appSettings)
       where TAppSettings : class, IViewsUrl
    {
        var useCors = IViewsUrl.UseCors(builder.ApplicationServices);
        if (useCors)
        {
            builder.UseCors();
        }
        return builder;
    }
}