using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using System.Security.Cryptography;

namespace AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;

public interface IAppVerCoreService
{
    /// <summary>
    /// 用于在浏览器中打开的网页上通过 Query 获取的安全信息参数值
    /// <para>在 Items 中缓存，适用于在中间件与控制器中或者其他服务中使用</para>
    /// </summary>
    Task<(string? ErrorMessage, Aes? Aes)> GetSecurityResultAsync(HttpContext context);

    /// <summary>
    /// 从 HTTP 请求上下文获取 App 版本信息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="fromHeaderOrQuery"></param>
    /// <returns></returns>
    Task<IReadOnlyAppVer?> GetAsync(HttpContext context, bool fromHeaderOrQuery);

    /// <summary>
    /// 根据版本号字符串获取 App 版本信息，如果不存在则生成一个空的实现接口，且 <see cref="IReadOnlyAppVer.Version"/> 值为传入参数
    /// </summary>
    /// <param name="appVersion"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyAppVer?> GetAsync(string appVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据主键查找 App 版本信息
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<IReadOnlyAppVer?> FindAsync(Guid id, CancellationToken cancellationToken = default);
}

#if DEBUG
[Obsolete("use IAppVerCoreService", true)]
public interface IAppVerService : IAppVerCoreService
{
}
#endif