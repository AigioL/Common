using System.Diagnostics;
using System.Text.Json.Serialization;

namespace System.Management.NetworkInformation;

/// <summary>
/// 网络适配器信息模型类
/// </summary>
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public sealed record class NetAdapterInfo
{
    /// <summary>
    /// 适配器名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 接口描述
    /// </summary>
    public string? InterfaceDescription { get; set; }

    /// <summary>
    /// 接口索引
    /// </summary>
    public uint InterfaceIndex { get; set; }

    /// <summary>
    /// 管理状态 (1=启用, 2=禁用)
    /// </summary>
    public uint AdminStatus { get; set; }

    /// <summary>
    /// 操作状态 (1=运行, 2=断开, 等)
    /// </summary>
    public uint OperationalStatus { get; set; }

    /// <summary>
    /// 链路速度 (bps)
    /// </summary>
    public ulong LinkSpeed { get; set; }

    /// <summary>
    /// MAC 地址
    /// </summary>
    public string? MacAddress { get; set; }

    /// <summary>
    /// 驱动程序版本
    /// </summary>
    public string? DriverVersion { get; set; }

    /// <summary>
    /// 驱动程序名称
    /// </summary>
    public string? DriverName { get; set; }

    /// <summary>
    /// 是否为虚拟适配器
    /// </summary>
    public bool Virtual { get; set; }

    /// <summary>
    /// 是否存在连接器
    /// </summary>
    public bool ConnectorPresent { get; set; }

    /// <summary>
    /// 管理状态文本
    /// </summary>
    [JsonIgnore]
    public string AdminStatusText => AdminStatus switch
    {
        1 => "启用",
        2 => "禁用",
        _ => "未知"
    };

    /// <summary>
    /// 操作状态文本
    /// </summary>
    [JsonIgnore]
    public string OperationalStatusText => OperationalStatus switch
    {
        1 => "运行",
        2 => "断开",
        3 => "连接中",
        4 => "不可用",
        _ => "未知"
    };

    /// <summary>
    /// 格式化的链路速度
    /// </summary>
    [JsonIgnore]
    public string FormattedLinkSpeed
    {
        get
        {
            if (LinkSpeed == 0) return "未知";

            var speedMbps = LinkSpeed / 1_000_000.0;
            if (speedMbps >= 1000)
                return $"{speedMbps / 1000:F1} Gbps";
            else
                return $"{speedMbps:F0} Mbps";
        }
    }

    string DebuggerDisplay() => $"{Name} ({InterfaceDescription}) - {AdminStatusText}/{OperationalStatusText} - {FormattedLinkSpeed} | {ToString()}";
}