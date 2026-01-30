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
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Channels;

namespace AigioL.Common.AspNetCore.AppCenter.Workers.Abstractions;

public abstract partial class CallbackWorkerBackGroundService(
    ILogger logger,
    IOptions<JsonOptions> jsonOptions,
    IConnection rabbitmqConn,
    IFeishuApiClient feishuApiClient) : ManualWorkerBackgroundService(logger, jsonOptions, rabbitmqConn, feishuApiClient)
{
    protected readonly ConcurrentDictionary<Guid, TaskCompletionSource> _callbackMapper = new();

    IChannel? callbackChannel;

    IChannel CallbackChannel
    {
        get
        {
            ArgumentNullException.ThrowIfNull(callbackChannel);
            return callbackChannel;
        }
    }

    protected abstract string CallbackTaskKey { get; }

    protected virtual string CallbackExchangeName { get; } = "temporary.amq.direct";

    /// <summary>
    /// 默认回调等待 1 分钟
    /// </summary> 
    protected virtual TimeSpan CallbackTaskTimeout { get; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// 处理回调消息，通知等待线程继续
    /// </summary>
    /// <returns></returns>
    public async Task HandleCallbackAsync(BasicDeliverEventArgs eventArgs, IChannel channel, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(eventArgs.Body.Span, out Guid taskId))
        {
            if (!_callbackMapper.TryRemove(taskId, out var tcs))
                return;
            tcs.SetResult();
        }
        //无效消息抛弃
        await channel.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        #region 绑定需要的回调队列
        try
        {
            var RunTaskId = Guid.NewGuid();
            var callbackRoutingKey = $"Callback.{CallbackTaskKey}.{RunTaskId}";
            var callbackQueueName = $"Callback.Queue.{CallbackTaskKey}.{RunTaskId}";
            callbackChannel = await rabbitmqConn.CreateChannelAsync(cancellationToken: cancellationToken);
            await callbackChannel.ExchangeDeclareAsync(CallbackExchangeName, ExchangeType.Direct, durable: true);
            await callbackChannel.QueueDeclareAsync(callbackQueueName, durable: false, exclusive: true, autoDelete: true, arguments: null);
            await callbackChannel.QueueBindAsync(callbackQueueName, CallbackExchangeName, callbackRoutingKey, cancellationToken: cancellationToken);
            var callbackConsumer = new AsyncEventingBasicConsumer(callbackChannel);
            callbackConsumer.ReceivedAsync += async (s, e) =>
            {
                await HandleCallbackAsync(e, callbackChannel, cancellationToken);
            };
            await callbackChannel.BasicConsumeAsync(callbackQueueName, false, callbackConsumer, cancellationToken: cancellationToken);
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
                    await OnNotificationAsync($"Callback WorkerExecuteErr: {WorkerName}", ex.ToString());
                }
            }
        }
        #endregion
        #region 正常注册的 队列
        await base.ExecuteAsync(cancellationToken);
        #endregion
    }
}
