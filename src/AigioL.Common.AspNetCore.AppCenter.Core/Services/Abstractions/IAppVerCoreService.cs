using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;

public interface IAppVerCoreService
{
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
