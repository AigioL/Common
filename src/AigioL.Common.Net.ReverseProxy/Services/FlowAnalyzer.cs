using AigioL.Common.Net.ReverseProxy.Internals.FlowAnalyzer;
using AigioL.Common.Net.ReverseProxy.Models;
using AigioL.Common.Net.ReverseProxy.Services.Abstractions;

namespace AigioL.Common.Net.ReverseProxy.Services;

/// <summary>
/// <see cref="IFlowAnalyzer"/> 的默认实现类
/// </summary>
sealed class FlowAnalyzer : IFlowAnalyzer
{
    const int INTERVAL_SECONDS = 5;

    readonly FlowQueues readQueues = new(INTERVAL_SECONDS);
    readonly FlowQueues writeQueues = new(INTERVAL_SECONDS);

    /// <inheritdoc/>
    public void OnFlow(FlowType flowType, int length)
    {
        if (flowType == FlowType.Read)
        {
            readQueues.OnFlow(length);
        }
        else
        {
            writeQueues.OnFlow(length);
        }
    }

    /// <inheritdoc/>
    public FlowStatistics GetFlowStatistics() => new()
    {
        TotalRead = readQueues.TotalBytes,
        TotalWrite = writeQueues.TotalBytes,
        ReadRate = readQueues.GetRate(),
        WriteRate = writeQueues.GetRate(),
    };
}
