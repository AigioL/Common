using AigioL.Common.AspNetCore.AppCenter.Identity.UI.Properties;

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

    /// <summary>
    /// html 的 meta theme-color
    /// </summary>
    public string MetaThemeColor
    {
        get => field ?? Resources.ColorPrimary;
        init;
    }

    /// <summary>
    /// html 的 meta msapplication-TileColor
    /// </summary>
    public string MetaMSApplicationTileColor
    {
        get => field ?? Resources.ColorPrimary;
        init;
    }

    /// <summary>
    /// html 的 meta msapplication-window
    /// </summary>
    public string MetaMSApplicationWindow
    {
        get => field ?? Resources.MetaMSApplicationWindow;
        init;
    }

    /// <summary>
    /// html 的 head title
    /// </summary>
    public string? HeadTitle { get; set; }

    /// <summary>
    /// html 的 body noscript
    /// </summary>
    public string NoScript
    {
        get => field ?? Resources.BodyNoScript;
        init;
    }
}