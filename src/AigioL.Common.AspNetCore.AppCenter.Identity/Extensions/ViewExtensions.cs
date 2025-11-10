using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Shared;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.AppCenter;

/// <summary>
/// 页面视图相关的扩展函数
/// </summary>
public static partial class ViewExtensions
{
    /// <summary>
    /// 获取页面布局布局模型，优先从缓存中获取
    /// </summary>
    /// <param name="repo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<LayoutModel> GetLayoutModelAsync(
#if DEBUG
        this IKeyValuePairRepository? repo,
#else
        this IKeyValuePairRepository repo,
#endif
        CancellationToken cancellationToken = default)
    {
        (var layoutModel, var htmlLang) =
#if DEBUG
            repo == null ? (null, "zh-CN") :
#endif
            await repo.GetViewLayoutModelAsync(cancellationToken);

        var metaDescription = layoutModel?.MetaDescription;
        var metaKeywords = layoutModel?.MetaKeywords;
        var appName = layoutModel?.AppName;

        LayoutModel layout = new()
        {
            HtmlLang = htmlLang,
            MetaDescription = metaDescription ?? "",
            MetaKeywords = metaKeywords ?? "",
            AppName = appName ?? "",
        };
        return layout;
    }
}
