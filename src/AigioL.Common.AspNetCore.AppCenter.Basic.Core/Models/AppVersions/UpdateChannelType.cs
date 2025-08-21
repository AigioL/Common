namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions;

/// <summary>
/// 下载更新渠道类型
/// </summary>
public enum UpdateChannelType : byte
{
    /// <summary>
    /// 自动选择渠道
    /// </summary>
    Auto,

    /// <summary>
    /// https://github.com/BeyondDimension/SteamTools/releases
    /// </summary>
    GitHub,

    /// <summary>
    /// 从官方服务器上下载更新
    /// </summary>
    Official = byte.MaxValue,
}
