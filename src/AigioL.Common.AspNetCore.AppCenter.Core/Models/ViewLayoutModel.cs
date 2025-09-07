namespace AigioL.Common.AspNetCore.AppCenter.Models;

/// <summary>
/// 通用页面布局模型类
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class ViewLayoutModel
{
    [global::MemoryPack.MemoryPackOrder(0)]
    public required string AppName { get; set; }

    /// <summary>
    /// html 的 meta keywords
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public required string MetaKeywords { get; set; }

    /// <summary>
    /// html 的 meta description
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public required string MetaDescription { get; set; }

    /// <summary>
    /// html 的 meta theme-color
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public string? MetaThemeColor { get; set; }

    /// <summary>
    /// html 的 meta msapplication-TileColor
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(4)]
    public string? MetaMSApplicationTileColor { get; set; }

    /// <summary>
    /// html 的 meta msapplication-window
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(5)]
    public string? MetaMSApplicationWindow { get; set; }

    /// <summary>
    /// html 的 body noscript
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(6)]
    public string? NoScript { get; set; }
}
