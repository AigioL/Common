namespace AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus;

/// <summary>
/// 广告类型
/// </summary>
public enum KomaasharuType
{
    /// <summary>
    /// 横幅广告
    /// </summary>
    Banner = 1,

    /// <summary>
    /// 程序内弹窗广告
    /// </summary>
    Popup = 2,

    /// <summary>
    /// 系统消息广告
    /// </summary>
    News = 4,

    /// <summary>
    /// 内嵌广告
    /// </summary>
    Embedded = 8,

    /// <summary>
    /// 主页
    /// </summary>
    HomePage = 16,
}