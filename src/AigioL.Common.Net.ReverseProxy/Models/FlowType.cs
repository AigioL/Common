namespace AigioL.Common.Net.ReverseProxy.Models;

/// <summary>
/// 流量类型
/// </summary>
public enum FlowType : byte
{
    /// <summary>
    /// 读取
    /// </summary>
    Read,

    /// <summary>
    /// 写入
    /// </summary>
    Wirte,
}