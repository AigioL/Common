using Microsoft.AspNetCore.Cors.Infrastructure;

namespace AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

public partial interface IViewsUrl
{
    /// <summary>
    /// 配置允许跨域访问的 Web UI 地址
    /// </summary>
    string? ViewsUrl { get; }

    string[] GetOrigins() => GetOrigins(ViewsUrl);

    static string[] GetOrigins(string? viewsUrl)
    {
        if (string.IsNullOrWhiteSpace(viewsUrl))
        {
            return [];
        }
        else
        {
            return [.. viewsUrl.Split([',', ';', '|', '，', '；'], StringSplitOptions.RemoveEmptyEntries).Where(x => x.IsHttpUrl())];
        }
    }

    /// <summary>
    /// 是否使用跨域访问，调用了 AddCors
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    static bool UseCors(IServiceProvider serviceProvider)
    {
        var s = serviceProvider.GetServices<ICorsService>();
        return s != null;
    }
}