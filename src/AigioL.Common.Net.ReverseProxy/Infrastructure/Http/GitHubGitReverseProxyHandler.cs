using AigioL.Common.Net.ReverseProxy.Infrastructure.NameResolution;
using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Http;

/// <summary>
/// Github Git 代理处理
/// </summary>
sealed class GitHubGitReverseProxyHandler(IDomainResolver domainResolver)
    : TcpReverseProxyHandler(domainResolver, new("github.com", GitHubDesktopPort));