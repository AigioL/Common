using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.PacketIntercept;

/// <summary>
/// SSH 端口 TCP 拦截器
/// </summary>
sealed class SshInterceptor(ILogger<SshInterceptor> logger, ushort newServerPort) : TcpInterceptor(logger, SshPort, newServerPort);

static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddSshInterceptor(this IServiceCollection services, ushort newServerPort)
    {
        if (newServerPort != SshPort)
        {
            services.AddSingleton<ITcpInterceptor, SshInterceptor>(provider => new SshInterceptor(provider.GetRequiredService<ILogger<SshInterceptor>>(), newServerPort));
        }
        return services;
    }
}