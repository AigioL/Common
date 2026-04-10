namespace AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus;

/// <summary>
/// 客户端点击广告后的动作
/// </summary>
public enum KomaasharuClientAction : byte
{
    /// <summary>
    /// 点击展示
    /// </summary>
    Display = 1,

    /// <summary>
    /// 点击弹窗下载
    /// </summary>
    PopupDownload = 2,

    /// <summary>
    /// 静默安装
    /// </summary>
    SilentInstall = 3,
}