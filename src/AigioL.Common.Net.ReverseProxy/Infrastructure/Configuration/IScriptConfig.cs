using System.Net;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Configuration;

public interface IScriptConfig
{
    /// <summary>
    /// 本地脚本 Id
    /// </summary>
    int LocalId { get; }

    /// <summary>
    /// 排除匹配域名
    /// </summary>
    DomainPattern? ExcludeDomainPattern { get; }
}
