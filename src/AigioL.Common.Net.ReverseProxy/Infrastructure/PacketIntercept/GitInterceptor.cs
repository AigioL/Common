using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.PacketIntercept;

/// <summary>
/// Git 端口 TCP 拦截器
/// </summary>
sealed class GitInterceptor(ILogger<GitInterceptor> logger, ushort newServerPort) : TcpInterceptor(logger, GitHubDesktopPort, newServerPort);

static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddGitInterceptor(this IServiceCollection services, ushort newServerPort)
    {
        if (newServerPort != GitHubDesktopPort)
        {
            services.AddSingleton<ITcpInterceptor, GitInterceptor>(provider => new GitInterceptor(provider.GetRequiredService<ILogger<GitInterceptor>>(), newServerPort));
        }
        return services;
    }
}