using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.PacketIntercept;

/// <summary>
/// HTTP 端口 TCP 拦截器
/// </summary>
sealed class HttpInterceptor(ILogger<HttpInterceptor> logger, ushort newServerPort) : TcpInterceptor(logger, HTTP_PORT, newServerPort);

static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddHttpInterceptor(this IServiceCollection services, ushort newServerPort)
    {
        if (newServerPort != HTTP_PORT)
        {
            services.AddSingleton<ITcpInterceptor, HttpInterceptor>(provider => new HttpInterceptor(provider.GetRequiredService<ILogger<HttpInterceptor>>(), newServerPort));
        }
        return services;
    }
}