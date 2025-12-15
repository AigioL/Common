using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using Microsoft.IO;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Services;

sealed partial class PaymentMessageQueueService(IConnection rabbitmqConn) : IPaymentMessageQueueService
{
    readonly RecyclableMemoryStreamManager m = new();

    const string exchange = ""; // 默认交换机

    async Task ListRightPushAsync(
        string routingKey,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken = default)
    {
        using var channel = await rabbitmqConn.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.BasicPublishAsync(exchange, routingKey, body, cancellationToken);
    }

    public async Task PushPaymentSuccess(OrderPaymentSuccessInfo info)
    {
        using var stream = m.GetStream();
        await JsonSerializer.SerializeAsync(stream, info,
            PaymentMinimalApisJsonSerializerContext.Default.OrderPaymentSuccessInfo);
        var value = stream.GetMemory();
        await ListRightPushAsync(CacheKeys.OrderPaymentSuccess, value);
    }

    public async Task PushRefundSuccess(OrderRefundSuccessInfo info)
    {
        using var stream = m.GetStream();
        await JsonSerializer.SerializeAsync(stream, info,
            PaymentMinimalApisJsonSerializerContext.Default.OrderRefundSuccessInfo);
        var value = stream.GetMemory();
        await ListRightPushAsync(CacheKeys.OrderRefundSuccess, value);
    }

    public async Task PushSignAgreementSuccess(string agreementNo)
    {
        var value = Encoding.UTF8.GetBytes(agreementNo);
        await ListRightPushAsync(CacheKeys.AgreementSignSuccessInfo, value);
    }

    public async Task PushUnSignAgreementSuccess(string agreementNo)
    {
        var value = Encoding.UTF8.GetBytes(agreementNo);
        await ListRightPushAsync(CacheKeys.AgreementUnSignSuccessInfo, value);
    }
}
