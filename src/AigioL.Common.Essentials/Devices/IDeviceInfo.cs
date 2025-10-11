using AigioL.Common.Primitives.Models;

namespace AigioL.Common.Essentials.Devices;

/// <summary>
/// 表示有关设备的信息
/// </summary>
public partial interface IDeviceInfo
{
    /// <summary>
    /// 获取运行应用程序的设备的类型
    /// </summary>
    DeviceType DeviceType { get; }

    /// <summary>
    /// 获取设备的成语 (外形规格)
    /// </summary>
    DeviceIdiom Idiom { get; }

    /// <summary>
    /// 获取设备的制造商
    /// </summary>
    string Manufacturer { get; }

    /// <summary>
    /// 获取设备的型号
    /// </summary>
    string Model { get; }

    /// <summary>
    /// 获取设备的名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 获取设备的平台或操作系统
    /// </summary>
    DevicePlatform2 Platform { get; }

    /// <summary>
    /// 获取操作系统的版本
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// 获取操作系统版本的字符串表示形式
    /// </summary>
    string VersionString { get; }
}
