using AigioL.Common.Net.ReverseProxy.Infrastructure.NameResolution;
using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Http;

/// <summary>
/// Github SSH 代理
/// </summary>
sealed class GitHubSshReverseProxyHandler(IDomainResolver domainResolver)
    : TcpReverseProxyHandler(domainResolver, new("github.com", SshPort));