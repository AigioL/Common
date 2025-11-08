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
}