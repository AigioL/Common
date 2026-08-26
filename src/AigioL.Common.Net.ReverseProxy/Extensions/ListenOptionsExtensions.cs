using AigioL.Common.Net.ReverseProxy.Internals.FlowAnalyzer;
using AigioL.Common.Net.ReverseProxy.Services.Abstractions;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Hosting;

/// <summary>
/// <see cref="ListenOptions"/> 的扩展方法
/// </summary>
public static partial class ListenOptionsExtensions
{
    /// <summary>
    /// 使用流量分析中间件
    /// </summary>
    public static ListenOptions UseFlowAnalyze(this ListenOptions listen)
    {
        var flowAnalyzer = listen.ApplicationServices.GetRequiredService<IFlowAnalyzer>();
        listen.Use(next => async context =>
        {
            var oldTransport = context.Transport;
            try
            {
                await using var adapter = new DuplexPipeStreamAdapter<FlowAnalyzeStream>(
                    context.Transport,
                    stream => new(stream, flowAnalyzer));
                context.Transport = adapter;
                await next(context);
            }
            finally
            {
                context.Transport = oldTransport;
            }
        });
        return listen;
    }
}
