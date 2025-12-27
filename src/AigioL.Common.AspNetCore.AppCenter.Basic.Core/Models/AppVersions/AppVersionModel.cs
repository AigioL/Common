using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions;

/// <summary>
/// 应用程序版本号模型
/// </summary>
#if !NETFRAMEWORK
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
#endif
public sealed partial record class AppVersionModel : IExplicitHasValue
{
    /// <summary>
    /// 版本 Id
    /// </summary>
#if !NETFRAMEWORK
    [global::MemoryPack.MemoryPackOrder(0)]
#endif
    public Guid Id { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
#if !NETFRAMEWORK
    [global::MemoryPack.MemoryPackOrder(1)]
#endif
    public string Version { get; set; } = "";

    /// <summary>
    /// 版本说明
    /// </summary>
#if !NETFRAMEWORK
    [global::MemoryPack.MemoryPackOrder(2)]
#endif
    public string ReleaseNote { get; set; } = "";

    /// <summary>
    /// (单选)支持的平台
    /// </summary>
#if !NETFRAMEWORK
    [global::MemoryPack.MemoryPackOrder(3)]
#endif
#pragma warning disable CS0618 // 类型或成员已过时
    public WebApiCompatDevicePlatform Platform { get; set; }
#pragma warning restore CS0618 // 类型或成员已过时

    /// <summary>
    /// (多或单选)支持的 CPU 构架
    /// </summary>
#if !NETFRAMEWORK
    [global::MemoryPack.MemoryPackOrder(4)]
#endif
    public ArchitectureFlags SupportedAbis { get; set; }

    /// <summary>
    /// 是否禁用自动化更新，当此值为 <see langword="true"/> 时，仅提供跳转官网的手动更新方式
    /// </summary>
#if !NETFRAMEWORK
    [global::MemoryPack.MemoryPackOrder(5)]
#endif
    public bool DisableAutomateUpdate { get; set; }

    /// <summary>
    /// 下载类型与下载地址
    /// </summary>
#if !NETFRAMEWORK
    [global::MemoryPack.MemoryPackOrder(6)]
#endif
    public List<AppVersionDownloadModel>? Downloads { get; set; }

#if !NETFRAMEWORK
    [global::MemoryPack.MemoryPackOrder(7)]
#endif
    public DateTimeOffset PublishTime { get; set; }

    /// <inheritdoc/>
    bool IExplicitHasValue.ExplicitHasValue()
    {
        return !string.IsNullOrWhiteSpace(Version) &&
            !string.IsNullOrWhiteSpace(ReleaseNote) &&
            (DisableAutomateUpdate || (Downloads != null && Downloads.Any(x => x.HasValue())));
    }
}

public sealed partial class AppVersionTableItemModel : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    /// <summary>
    /// 私钥
    /// </summary>
    public string? PrivateKey { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { get; set; } = "";

    /// <summary>
    /// 发布说明
    /// </summary>
    public string ReleaseNote { get; set; } = "";

    /// <summary>
    /// 是否禁用
    /// </summary>
    public bool Disable { get; set; }

    /// <summary>
    /// 是否Beta版本
    /// </summary>
    public bool BetaVersion { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset UpdateTime { get; set; }

    /// <summary>
    /// 创建人 UserId（创建此条目的后台管理员）
    /// </summary>
    public Guid? CreateUserId { get; set; }

    /// <summary>
    /// 创建人（创建此条目的后台管理员）
    /// </summary>
    public string? CreateUser { get; set; }

    /// <summary>
    /// 最后一次操作的人 UserId（记录后台管理员禁用或启用或编辑该条的操作）
    /// </summary>
    public Guid? OperatorUserId { get; set; }

    /// <summary>
    /// 最后一次操作的人（记录后台管理员禁用或启用或编辑该条的操作）
    /// </summary>
    public string? OperatorUser { get; set; }

    /// <summary>
    /// 可更新时间
    /// </summary>
    public DateTimeOffset? AvailableTime { get; set; }

    /// <summary>
    /// 客户端设备
    /// </summary>
    public ClientPlatform ClientDeviceType { get; set; }
}

public sealed partial class AddOrEditAppVersionModel : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    /// <summary>
    /// 私钥
    /// </summary>
    public string? PrivateKey { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    [StringLength(maximumLength: MaxLengths.Version, MinimumLength = 1)]
    public string Version { get; set; } = "";

    /// <summary>
    /// 发布说明
    /// </summary>
    [Required]
    public string ReleaseNote { get; set; } = "";

    /// <summary>
    /// 是否禁用
    /// </summary>
    public bool Disable { get; set; }

    /// <summary>
    /// 是否Beta版本
    /// </summary>
    public bool BetaVersion { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreateTime { get; set; }

    /// <summary>
    /// 可更新时间
    /// </summary>
    public DateTimeOffset? AvailableTime { get; set; }

    /// <summary>
    /// 客户端设备
    /// </summary>
    public ClientPlatform ClientDeviceType { get; set; }
}