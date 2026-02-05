using AigioL.Common.Primitives.Columns;
using System.Security.Cryptography;
using static AigioL.Common.AspNetCore.AppCenter.Models.Abstractions.DeviceIdExtensions;

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

    static string? GetDeviceId(Guid deviceIdG, string? deviceIdR, string? deviceIdN)
    {
        if (deviceIdG != default && IsDeviceIdR(deviceIdR) &&
           IsDeviceIdN(deviceIdN))
        {
            var r = ShortGuid.Encode(deviceIdG) + deviceIdR + deviceIdN;
            return r;
        }
        return null;
    }
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
        var deviceIdString = IDeviceId.GetDeviceId(deviceId.DeviceIdG, deviceId.DeviceIdR, deviceId.DeviceIdN);
        return deviceIdString;
    }
}