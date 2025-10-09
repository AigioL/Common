using System.Collections.Immutable;

namespace AigioL.Common.Essentials.ApplicationModel;

/// <summary>
/// VersionTracking API 提供了一种在设备上跟踪应用版本的简单方法
/// </summary>
public interface IVersionTracking
{
    /// <summary>
    /// 获取在此设备上运行的应用的内部版本号集合
    /// </summary>
    ImmutableArray<string> BuildHistory { get; }

    /// <summary>
    /// 获取应用的当前内部版本
    /// </summary>
    string CurrentBuild { get; }

    /// <summary>
    /// 获取应用的当前版本号
    /// </summary>
    string CurrentVersion { get; }

    /// <summary>
    /// 获取此设备上安装的应用的第一个版本的内部版本号
    /// </summary>
    string? FirstInstalledBuild { get; }

    /// <summary>
    /// 获取此设备上安装的应用的第一个版本的版本号
    /// </summary>
    string? FirstInstalledVersion { get; }

    /// <summary>
    /// 获取一个值，该值指示这是否是第一次在此设备上启动此应用
    /// </summary>
    bool IsFirstLaunchEver { get; }

    /// <summary>
    /// 获取一个值，该值指示这是否是当前内部版本号的应用首次启动
    /// </summary>
    bool IsFirstLaunchForCurrentBuild { get; }

    /// <summary>
    /// 获取一个值，该值指示这是当前版本号的应用首次启动
    /// </summary>
    bool IsFirstLaunchForCurrentVersion { get; }

    /// <summary>
    /// 获取以前运行的版本的内部版本号
    /// </summary>
    string? PreviousBuild { get; }

    /// <summary>
    /// 获取以前运行的版本的版本号
    /// </summary>
    string? PreviousVersion { get; }

    /// <summary>
    /// 获取在此设备上运行的应用的版本号集合
    /// </summary>
    ImmutableArray<string> VersionHistory { get; }

    /// <summary>
    /// 确定这是否是针对指定内部版本号首次启动应用
    /// </summary>
    /// <param name="build"></param>
    /// <returns></returns>
    bool IsFirstLaunchForBuild(string build);

    /// <summary>
    /// 确定这是否是针对指定版本号首次启动应用
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    bool IsFirstLaunchForVersion(string version);

    ///// <summary>
    ///// 开始跟踪版本信息
    ///// </summary>
    //void Track(); 无需显式调用
}
