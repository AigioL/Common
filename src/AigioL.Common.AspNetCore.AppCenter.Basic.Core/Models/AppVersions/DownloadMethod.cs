namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions;

/// <summary>
/// 文件下载方式
/// </summary>
public enum DownloadMethod : byte
{
    /// <summary>
    /// HTTP 下载
    /// </summary>
    Http = 1,

    /// <summary>
    /// BT 下载
    /// </summary>
    BitTorrent = 2,
}