using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.FeishuOApi.Sdk.Services.Abstractions;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;

namespace AigioL.Common.AspNetCore.AppCenter.Workers.Abstractions;

/// <summary>
/// 手动处理 RabbitMQ 消息的后台服务基类
/// </summary>
/// <param name="logger"></param>
/// <param name="jsonOptions"></param>
/// <param name="rabbitmqConn"></param>
/// <param name="feishuApiClient"></param>
public abstract partial class ManualWorkerBackgroundService(
    ILogger logger,
    IOptions<JsonOptions> jsonOptions,
    IConnection rabbitmqConn,
    IFeishuApiClient feishuApiClient) : BackgroundService
{
    /// <inheritdoc cref="ILogger"/>
    protected readonly ILogger logger = logger;

    readonly JsonSerializerOptions serializerOptions = jsonOptions.Value.SerializerOptions;

    /// <inheritdoc cref="IFeishuApiClient"/>
    protected readonly IFeishuApiClient feishuApiClient = feishuApiClient;

    protected readonly IConnection rabbitmqConn = rabbitmqConn;

    protected JsonTypeInfo GetTypeInfo(Type type) => serializerOptions.GetTypeInfo(type);

    protected JsonTypeInfo<T> GetTypeInfo<T>() => (JsonTypeInfo<T>)serializerOptions.GetTypeInfo(typeof(T));

    /// <summary>
    /// 业务逻辑执行入口，由子类重写实现
    /// </summary>
    protected abstract Task<ApiRsp> HandleAsync(BasicDeliverEventArgs eventArgs, IChannel channel, CancellationToken cancellationToken);

    protected IChannel? channel;

    protected IChannel Channel
    {
        get
        {
            ArgumentNullException.ThrowIfNull(channel);
            return channel;
        }
    }

    protected abstract string RoutingKey { get; }

    protected abstract string QueueName { get; }

    protected virtual string ExchangeName { get; } = "amq.direct";

    /// <summary>
    /// 此消费者标签用于标识订阅，以及需要取消订阅的时机
    /// </summary>
    protected string ConsumerTag
    {
        get
        {
            ArgumentNullException.ThrowIfNull(field);
            return field;
        }
        private set;
    }

    protected virtual string WorkerName
    {
        get
        {
            var typeName = GetType().Name;
            const string trimChars = "Worker";
            if (typeName.Length != trimChars.Length &&
                typeName.EndsWith(trimChars, StringComparison.InvariantCultureIgnoreCase))
            {
                return typeName[..^trimChars.Length];
            }
            return typeName;
        }
    }

    /// <summary>
    /// 是否通知
    /// </summary>
    protected virtual bool Notification => false;

    /// <summary>
    /// 是否仅在失败时通知
    /// </summary>
    protected virtual bool NotificationOnlyFail => true;

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var workerName = WorkerName;
        // https://github.com/ryanninodizon/AspireWithRabbitMQ/blob/b36fa2a0b7c0a4b89add0dde341ea3139aee061e/AspireWithRabbitmQ.Receiver/ProcessRabbitMQMessage.cs
        // https://rabbitmq.cn/client-libraries/dotnet-api-guide#consuming
        try
        {
            var queueName = QueueName;
            var exchangeName = ExchangeName;
            var routingKey = RoutingKey;
            channel = await rabbitmqConn.CreateChannelAsync(cancellationToken: cancellationToken);
            await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true);
            await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await channel.QueueBindAsync(queueName, exchangeName, routingKey, cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (s, e) =>
            {
                ApiRsp? r = null;
                try
                {
                    r = await HandleAsync(e, channel, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    r = ex;
                }
                //错误信息发送消息通知
                if (r != null && !r.IsSuccess())
                {
                    await OnHandleFailAsync(workerName, r);
                }
            };

            // this consumer tag identifies the subscription
            // when it has to be cancelled
            ConsumerTag = await channel.BasicConsumeAsync(queueName, false, consumer, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            if (Notification)
            {
                if (!NotificationOnlyFail)
                {
                    await OnNotificationAsync($"WorkerExecuteErr: {workerName}", ex.ToString());
                }
            }
        }
    }

    protected virtual async Task PushRetryTaskMassge(BasicDeliverEventArgs eventArgs, IChannel channel, Memory<byte> value, CancellationToken cancellationToken)
    {
        await channel.BasicPublishAsync(ExchangeName, RoutingKey, value, cancellationToken);
        //确认掉当前消息 后续任务处理 新消息
        await channel.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken: CancellationToken.None);
    }

    protected virtual async Task OnHandleFailAsync(
        string workerName,
        ApiRsp? r)
    {
        if (Notification)
        {
            if (!NotificationOnlyFail)
            {
                var message = r == null ? null : JsonSerializer.Serialize(r, MSMinimalApisJsonSerializerContext.Default.ApiRsp);
                await OnNotificationAsync($"WorkerHandleErr: {workerName}", message);
            }
        }
    }

    protected virtual async Task OnHandleFailAsync(
        string workerName,
        string? message)
    {
        if (Notification)
        {
            if (!NotificationOnlyFail)
            {
                await OnNotificationAsync($"WorkerHandleErr: {workerName}", message);
            }
        }
    }

    protected virtual async Task OnNotificationAsync(
        string title,
        string? message)
    {
        var result = await feishuApiClient.SendMessageAsync(title, message, CancellationToken.None);
    }
}
