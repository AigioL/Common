using AigioL.Common.AspNetCore.AppCenter.Basic.Models.FileSystem;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions;

/// <summary>
/// 应用程序版本号下载信息模型
/// </summary>
#if !NETFRAMEWORK
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
#endif

public sealed partial class AppVersionDownloadModel : IExplicitHasValue
{
    /// <inheritdoc cref="CloudFileType"/>
#if !NETFRAMEWORK
    [global::MemoryPack.MemoryPackOrder(0)]
#endif
    public CloudFileType DownloadType { get; set; }

    /// <inheritdoc cref="System.Security.Cryptography.SHA256"/>
#if !NETFRAMEWORK
    [global::MemoryPack.MemoryPackOrder(1)]
#endif
    public string? SHA256 { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
#if !NETFRAMEWORK
    [global::MemoryPack.MemoryPackOrder(2)]
#endif
    public long Length { get; set; }

    /// <summary>
    /// 下载地址
    /// </summary>
#if !NETFRAMEWORK
    [global::MemoryPack.MemoryPackOrder(3)]
#endif
    public string DownloadUrl { get; set; } = string.Empty;

    /// <inheritdoc cref="UpdateChannelType"/>
#if !NETFRAMEWORK
    [global::MemoryPack.MemoryPackOrder(4)]
#endif
    public UpdateChannelType DownloadChannelType { get; set; }

    /// <inheritdoc cref="System.Security.Cryptography.SHA384"/>
#if !NETFRAMEWORK
    [global::MemoryPack.MemoryPackOrder(5)]
#endif
    public string? SHA384 { get; set; }

    /// <summary>
    /// 是否增量更新
    /// </summary>
#if !NETFRAMEWORK
    [global::MemoryPack.MemoryPackOrder(6)]
#endif
    public bool IncrementalUpdate { get; set; }

    /// <summary>
    /// 所需运行时的下载地址
    /// </summary>
#if !NETFRAMEWORK
    [global::MemoryPack.MemoryPackOrder(7)]
#endif
    public string? DotNetRuntimeDownloadAddress { get; set; }

    //    /// <summary>
    //    /// 匹配域名地址数组
    //    /// </summary>
    //#if !NETFRAMEWORK
    //    [global::MemoryPack.MemoryPackIgnore]
    //#endif
    //    public string[] DownloadUrlArray
    //        => ApiConstants.GetSplitValues(DownloadUrl);

    /// <inheritdoc/>
    bool IExplicitHasValue.ExplicitHasValue()
    {
        return ((SHA256 != null && SHA256.Length == global::System.Security.Cryptography.SHA256.HashSizeInBytes * 2) || (SHA384 != null && SHA384.Length == global::System.Security.Cryptography.SHA384.HashSizeInBytes * 2)) &&
            (!string.IsNullOrWhiteSpace(DownloadUrl) && DownloadUrl.StartsWith("https://")) &&
            Length > 0 &&
            Enum.IsDefined(DownloadType) &&
            DownloadChannelType != UpdateChannelType.Auto && Enum.IsDefined(DownloadChannelType);
    }
}