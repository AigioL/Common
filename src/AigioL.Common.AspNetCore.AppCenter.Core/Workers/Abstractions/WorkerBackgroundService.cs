using AigioL.Common.FeishuOApi.Sdk.Services.Abstractions;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace AigioL.Common.AspNetCore.AppCenter.Workers.Abstractions;

/// <summary>
/// 消息队列生产、消费模式的消费业务基类，提供 Json 序列化支持
/// </summary>
public abstract partial class WorkerBackgroundService(
    ILogger logger,
    IOptions<JsonOptions> jsonOptions,
    IFeishuApiClient feishuApiClient) : BackgroundService
{
    /// <inheritdoc cref="ILogger"/>
    protected readonly ILogger logger = logger;

    readonly JsonSerializerOptions serializerOptions = jsonOptions.Value.SerializerOptions;

    /// <inheritdoc cref="IFeishuApiClient"/>
    protected readonly IFeishuApiClient feishuApiClient = feishuApiClient;

    protected JsonTypeInfo GetTypeInfo(Type type) => serializerOptions.GetTypeInfo(type);

    protected JsonTypeInfo<T> GetTypeInfo<T>() => (JsonTypeInfo<T>)serializerOptions.GetTypeInfo(typeof(T));

    /// <summary>
    /// 业务逻辑执行入口，由子类重写实现
    /// </summary>
    protected abstract Task HandleAsync(CancellationToken cancellationToken);

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await HandleAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {

        }
    }
}
