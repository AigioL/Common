using AigioL.Common.Primitives.Columns;
using System.Security.Cryptography;

namespace AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

public interface IDeviceId
{
    /// <summary>
    /// 设备标识符 G
    /// </summary>
    Guid DeviceIdG { get; set; }

    /// <summary>
    /// 设备标识符 R
    /// </summary>
    string? DeviceIdR { get; set; }

    /// <summary>
    /// 设备标识符 N
    /// </summary>
    string? DeviceIdN { get; set; }
}

public static partial class DeviceIdExtensions
{
    /// <summary>
    /// 字符串是否为设备标识符 R
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsDeviceIdR(string? value)
        => value != null && value.Length == MaxLengths.DeviceIdR
        && value.All(char.IsLetterOrDigit);

    /// <summary>
    /// 字符串是否为设备标识符 N
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsDeviceIdN(string? value)
        => value != null && value.Length == SHA256.HashSizeInBytes * 2
        && value.All(char.IsLetterOrDigit);

    public static string? GetDeviceId(this IDeviceId deviceId)
    {
        if (deviceId.DeviceIdG != default && IsDeviceIdR(deviceId.DeviceIdR) &&
            IsDeviceIdN(deviceId.DeviceIdN))
        {
            var r = ShortGuid.Encode(deviceId.DeviceIdG) + deviceId.DeviceIdR + deviceId.DeviceIdN;
            return r;
        }
        return null;
    }
}