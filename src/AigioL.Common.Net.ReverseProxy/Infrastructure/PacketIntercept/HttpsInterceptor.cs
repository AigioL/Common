using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.PacketIntercept;

/// <summary>
/// HTTPS 端口 TCP 拦截器
/// </summary>
sealed class HttpsInterceptor(ILogger<HttpsInterceptor> logger, ushort newServerPort) : TcpInterceptor(logger, HTTPS_PORT, newServerPort);

static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddHttpsInterceptor(this IServiceCollection services, ushort newServerPort)
    {
        if (newServerPort != HTTPS_PORT)
        {
            services.AddSingleton<ITcpInterceptor, HttpsInterceptor>(provider => new HttpsInterceptor(provider.GetRequiredService<ILogger<HttpsInterceptor>>(), newServerPort));
        }
        return services;
    }
}