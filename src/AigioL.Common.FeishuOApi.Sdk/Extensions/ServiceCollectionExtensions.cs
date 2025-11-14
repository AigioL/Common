using AigioL.Common.FeishuOApi.Sdk.Services;
using AigioL.Common.FeishuOApi.Sdk.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using System.Net;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionServiceExtensions
{
    /// <summary>
    /// 添加 <see cref="IFeishuApiClient"/> 客户端服务
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="baseAddress"></param>
    public static void AddFeishuApiClient(
        this IHostApplicationBuilder builder,
        string baseAddress = "https://open.feishu.cn")
    {
        builder.Services.AddHttpClient<IFeishuApiClient, FeishuApiClient>(client =>
        {
            client.BaseAddress = new(baseAddress);
        }).ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new SocketsHttpHandler
            {
                UseProxy = false,
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.None,
            };
            return handler;
        });
    }
}
