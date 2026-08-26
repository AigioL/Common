using AigioL.Common.Net.ReverseProxy.Models;

namespace AigioL.Common.Net.ReverseProxy.Services.Abstractions;

/// <summary>
/// 流量分析器
/// </summary>
public interface IFlowAnalyzer
{
    /// <summary>
    /// 收到数据
    /// </summary>
    void OnFlow(FlowType flowType, int length);

    /// <summary>
    /// 获取速率
    /// </summary>
    FlowStatistics GetFlowStatistics();
}