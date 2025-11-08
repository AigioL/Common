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
                services.AddSingleton(new UseCors());
            }
        }
    }

    /// <summary>
    /// 配置允许跨域的中间件
    /// </summary>
    public static IApplicationBuilder UseCors(this IApplicationBuilder builder, IViewsUrl appSettings)
    {
        if (!string.IsNullOrWhiteSpace(appSettings.ViewsUrl))
        {
            var s = builder.ApplicationServices.GetService<UseCors>();
            var useCors = s != null;
            if (useCors)
            {
                //var logger = builder.ApplicationServices.GetService<ILoggerFactory>()?.CreateLogger(nameof(IViewsUrl));
                //if (logger != null)
                //{
                //    LogCorsOrigins(logger, appSettings.ViewsUrl);
                //}
                builder.UseCors();
            }
        }
        return builder;
    }


    //[LoggerMessage(
    //    Level = LogLevel.Critical,
    //    Message = "已配置允许跨域访问的 Web UI 地址：{origins}")]
    //private static partial void LogCorsOrigins(ILogger logger, string? origins);
}

file sealed class UseCors();