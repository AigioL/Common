using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using WmiLight;

namespace System.Management.NetworkInformation;

/// <summary>
/// 网络适配器助手类，使用 MSFT_NetAdapter
/// </summary>
public static class NetAdapterHelper
{
    static ILogger Logger => field ?? Log.CreateLogger(nameof(NetAdapterHelper));

    static object? TryGetValue(WmiObject o, string key)
    {
        try
        {
            var result = o.GetPropertyValue(key);
            return result;
        }
        catch
        {
            // WmiLight key 不存在时抛出 COM 异常 0x80041002
            return null;
        }
    }

    /// <summary>
    /// 获取所有网络适配器信息
    /// </summary>
    /// <returns></returns>
    public static IReadOnlyList<NetAdapterInfo>? GetAll()
    {
        try
        {
            using WmiConnection conn = new(@"root\StandardCimv2");
            var query = conn.CreateQuery("SELECT * FROM MSFT_NetAdapter");
            List<NetAdapterInfo> result = new();
            foreach (var it in query)
            {
                var name = TryGetValue(it, "Name");
                var interfaceDescription = TryGetValue(it, "InterfaceDescription");
                var interfaceIndex = TryGetValue(it, "InterfaceIndex");
                var adminStatus = TryGetValue(it, "AdminStatus");
                var operationalStatus = TryGetValue(it, "OperationalStatus");
                var linkSpeed = TryGetValue(it, "LinkSpeed");
                var macAddress = TryGetValue(it, "PermanentAddress");
                var driverVersion = TryGetValue(it, "DriverVersion");
                var driverName = TryGetValue(it, "DriverName");
                var @virtual = TryGetValue(it, "Virtual");
                var connectorPresent = TryGetValue(it, "ConnectorPresent");

                NetAdapterInfo itM = new()
                {
                    Name = name?.ToString(),
                    InterfaceDescription = interfaceDescription?.ToString(),
                    InterfaceIndex = interfaceIndex != null ? Convert.ToUInt32(interfaceIndex) : 0,
                    AdminStatus = adminStatus != null ? Convert.ToUInt32(adminStatus) : 0,
                    OperationalStatus = operationalStatus != null ? Convert.ToUInt32(operationalStatus) : 0,
                    LinkSpeed = linkSpeed != null ? Convert.ToUInt64(linkSpeed) : 0,
                    MacAddress = macAddress?.ToString(),
                    DriverVersion = driverVersion?.ToString(),
                    DriverName = driverName?.ToString(),
                    Virtual = @virtual != null && Convert.ToBoolean(@virtual),
                    ConnectorPresent = connectorPresent != null && Convert.ToBoolean(connectorPresent),
                };
                result.Add(itM);
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.GetAllNetAdapterHelperException(ex);
            return null;
        }
    }

    /// <summary>
    /// 获取所有有线（排除 WiFi、蓝牙）的网络适配器的 MAC 地址哈希值
    /// </summary>
    public static string? GetMacAddressHash(ReadOnlySpan<byte> salt = default)
    {
        try
        {
            using WmiConnection conn = new(@"root\StandardCimv2");
            var query = conn.CreateQuery(
                "SELECT InterfaceDescription,PermanentAddress FROM MSFT_NetAdapter WHERE Virtual = FALSE");
            List<string> macAddresses = new();
            foreach (var it in query)
            {
                var macAddress = TryGetValue(it, "PermanentAddress")?.ToString();
                if (string.IsNullOrWhiteSpace(macAddress))
                {
                    continue;
                }
                var interfaceDescription = TryGetValue(it, "InterfaceDescription")?.ToString();
                if (interfaceDescription == null ||
                    "wireless".Contains(interfaceDescription, StringComparison.InvariantCultureIgnoreCase) ||
                    "wi-fi".Contains(interfaceDescription, StringComparison.InvariantCultureIgnoreCase) ||
                    "bluetooth".Contains(interfaceDescription, StringComparison.InvariantCultureIgnoreCase))
                {
                    // 跳过无线网卡和蓝牙网卡
                    continue;
                }
                macAddresses.Add(macAddress);
            }
            var sumlen = macAddresses.Sum(static x => x.Length);
            if (sumlen != 0)
            {
                // 计算所有 MAC 地址的字符总长度，获取 UTF8 编码的最大字节数建立缓冲区
                if (salt.IsEmpty)
                {
                    salt = "noitamrofnIkrowteN.tnemeganaM.metsyS.nommoC.LoigiA"u8; // 盐值，增加哈希的唯一性
                }
                var buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(sumlen) + salt.Length);
                try
                {
                    var s = buffer.AsSpan();
                    var writeCount = 0;
                    // 按降序写入 MAC 地址，确保相同的 MAC 地址顺序
                    foreach (var it in macAddresses.OrderByDescending(static x => x))
                    {
                        // 将 MAC 地址转换为 UTF8 字节数组
                        if (Encoding.UTF8.TryGetBytes(it, s, out var bytesWritten))
                        {
                            writeCount += bytesWritten;
                            s = s[bytesWritten..];
                        }
                    }
                    salt.CopyTo(buffer.AsSpan(writeCount)); // 将盐值追加到缓冲区末尾
                    s = buffer.AsSpan(0, writeCount + salt.Length);
                    // 使用 SHA384 哈希算法计算 MAC 地址的哈希值
                    Span<byte> result = stackalloc byte[SHA384.HashSizeInBytes];
                    SHA384.HashData(s, result);
                    return Convert.ToHexString(result);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.GetAllNetAdapterHelperException(ex);
        }
        return null;
    }
}

static partial class LoggerMessages
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "获取所有网络适配器信息失败")]
    internal static partial void GetAllNetAdapterHelperException(this ILogger logger, Exception exception);
}