namespace AigioL.Common.AspNetCore.AppCenter.Identity.UI.Slices;

/// <summary>
/// 页面布局模型类
/// </summary>
public sealed record class LayoutModel
{
    /// <summary>
    /// html 的 lang
    /// </summary>
    public required string HtmlLang { get; init; }

    /// <summary>
    /// html 的 meta keywords
    /// </summary>
    public required string MetaKeywords { get; init; }

    /// <summary>
    /// html 的 meta description
    /// </summary>
    public required string MetaDescription { get; init; }

    const string DefaultColor = "#2196F3";

    /// <summary>
    /// html 的 meta theme-color
    /// </summary>
    public string MetaThemeColor
    {
        get => field ?? DefaultColor;
        init;
    }

    /// <summary>
    /// html 的 meta msapplication-TileColor
    /// </summary>
    public string MetaMSApplicationTileColor
    {
        get => field ?? DefaultColor;
        init;
    }

    /// <summary>
    /// html 的 meta msapplication-window
    /// </summary>
    public string MetaMSApplicationWindow
    {
        get => field ?? "width=1024;height=768";
        init;
    }

    /// <summary>
    /// html 的 head title
    /// </summary>
    public required string HeadTitle { get; init; }

    /// <summary>
    /// html 的 body noscript
    /// </summary>
    public string NoScript
    {
        get => field ?? "需要开启 JavaScript 才能浏览本站点。";
        init;
    }
}
