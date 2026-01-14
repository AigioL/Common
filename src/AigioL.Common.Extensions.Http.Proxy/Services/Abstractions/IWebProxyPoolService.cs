using AigioL.Common.Models;
using System.Net;

namespace AigioL.Common.Extensions.Http.Proxy.Services.Abstractions;

public partial interface IWebProxyPoolService
{
    /// <summary>
    /// 使用传入的代理进行连接测试，返回是否成功与连接耗时
    /// </summary>
    Task<ApiRsp<TimeSpan?>> ConnectionTestAsync(
        IWebProxy webProxy,
        CancellationToken cancellationToken = default);
}
