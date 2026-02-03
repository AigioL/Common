using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.FeishuOApi.Sdk.Services.Abstractions;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Redd.Models.Steam;
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
    protected readonly ConcurrentDictionary<Guid, TaskCompletionSource<bool>> _callbackMapper = new();

    protected readonly Guid RunTaskId = Guid.NewGuid();

    IChannel? callbackChannel;

    protected abstract string CallbackTaskKey { get; }

    protected virtual string CallbackExchangeName { get; } = "temporary.amq.direct";

    public static string CallbackRoutingKey(string CallbackTaskKey, Guid RunTaskId) => $"Callback.{CallbackTaskKey}.{RunTaskId}";

    public static string CallbackQueueName(string CallbackTaskKey, Guid RunTaskId) => $"Callback.Queue.{CallbackTaskKey}.{RunTaskId}";

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
        try
        {
            var callbackInfo = JsonSerializer.Deserialize(eventArgs.Body.Span, MSMinimalApisJsonSerializerContext.Default.ApiRspGuid);
            if (callbackInfo != null)
            {
                if (!_callbackMapper.TryRemove(callbackInfo.Content, out var tcs))
                    return;
                tcs.SetResult(callbackInfo.IsSuccess());
            }
        }
        catch { }
        //无效消息抛弃
        await channel.BasicAckAsync(eventArgs.DeliveryTag, false, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        #region 绑定需要的回调队列
        try
        {
            var callbackRoutingKey = CallbackRoutingKey(CallbackTaskKey, RunTaskId);
            var callbackQueueName = CallbackQueueName(CallbackTaskKey, RunTaskId);
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
