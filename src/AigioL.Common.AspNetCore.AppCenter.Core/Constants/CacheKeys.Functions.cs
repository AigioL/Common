using AigioL.Common.AspNetCore.AppCenter.Basic.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Models;
using MemoryPack;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IO;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace AigioL.Common.AspNetCore.AppCenter.Constants;

static partial class CacheKeys
{
    /// <summary>
    /// 添加文章浏览量
    /// </summary>
    /// <returns></returns>
    public static async Task ArticleViewIncrementAsync(
        Guid id,
        IConnectionMultiplexer connection,
        CancellationToken cancellationToken = default)
    {
        var idString = id.ToString();
        var dbConnection = connection.GetDatabase(RedisHashIncrementDb, cancellationToken);
        await dbConnection.HashIncrementAsync(ArticleViewHashKey, idString);
    }

    /// <summary>
    /// 获取方法缓存 Key
    /// </summary>
    public static string GetMethodCacheKey(string methodName, params object?[] parameters)
    {
        const string separator = ", ";
        var parameters2 = parameters.Select(static p => p?.ToString()).ToArray();
        var len = methodName.Length + parameters2.Sum(static x => x == null ? 0 : x.Length) + ((parameters2.Length - 1) * separator.Length) + 2;
        var r = string.Create(len, parameters2, (chars, parameters2) =>
        {
            var temp = chars;
            methodName.AsSpan().CopyTo(temp);
            temp = temp[methodName.Length..];
            temp[0] = '(';
            temp = temp[1..];
            for (int i = 0; i < parameters2.Length; i++)
            {
                var param = parameters2[i];
                if (param != null)
                {
                    param.AsSpan().CopyTo(temp);
                    temp = temp[param.Length..];
                }
                if (i < parameters2.Length - 1)
                {
                    separator.AsSpan().CopyTo(temp);
                    temp = temp[separator.Length..];
                }
            }
            temp[0] = ')';
        });
        return r;
    }

    /// <summary>
    /// 获取支付服务状态
    /// </summary>
    /// <param name="distributedCache"></param>
    /// <returns></returns>
    public static async Task<bool> GetPaymentServiceStatus(IDistributedCache distributedCache)
    {
        bool enable = (await distributedCache.GetAsync(PaymentServiceStopped)) is null;
        return enable;
    }

    /// <summary>
    /// 设置支付服务状态
    /// </summary>
    /// <param name="distributedCache"></param>
    /// <param name="enable">是否启用</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static async Task SetPaymentServiceStatus(IDistributedCache distributedCache, bool enable)
    {
        if (enable)
        {
            // 移除停止标记
            await distributedCache.RemoveAsync(PaymentServiceStopped);
        }
        else
        {
            // 添加停止标记
            await distributedCache.SetAsync(PaymentServiceStopped, [1]);
        }
    }

    /// <summary>
    /// 竞争锁执行方法
    /// </summary>
    public static async Task<ApiRsp<T?>> LockHandleAsync<T>(
        this IConnectionMultiplexer connection,
        RedisKey lockKey,
        Func<Task<ApiRsp<T?>>> handle,
        TimeSpan? expiry = null,
        Func<Exception, Task<ApiRsp<T?>>>? errorHandle = null)
    {
        expiry ??= TimeSpan.FromMinutes(2);

        var lockValue = Guid.NewGuid().ToString("N");
        var lockDb = connection.GetDatabase(RedisLockDb);

        if (await lockDb.LockTakeAsync(lockKey, lockValue, expiry.Value))
        {
            try
            {
                return await handle();
            }
            catch (Exception ex) when (errorHandle is not null)
            {
                return await errorHandle(ex);
            }
            finally
            {
                await lockDb.LockReleaseAsync(lockKey, lockValue);
            }
        }

        return (ApiRspCode.TooManyRequests, "操作频繁，请稍后再试");
    }

    /// <summary>
    /// 获取缓存数据，缓存过期从数据库获取，避免缓存击穿
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="database"></param>
    /// <param name="cacheKey"></param>
    /// <param name="getData"></param>
    /// <param name="semaphoreSlim"></param>
    /// <param name="expiry">缓存数据过期时间，默认 1 小时</param>
    /// <param name="semaphoreSlimWaitMsTimeout">获取锁超时时间</param>
    /// <param name="expiryIsNull"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<T?> GetCacheDataAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        this IDatabase database,
        string cacheKey,
        Func<CancellationToken, Task<T>> getData,
        SemaphoreSlim? semaphoreSlim = null,
        TimeSpan? expiry = null,
        int semaphoreSlimWaitMsTimeout = 30,
        bool expiryIsNull = false,
        CancellationToken cancellationToken = default)
    {
        ReadOnlyMemory<byte> data = await database.StringGetAsync(cacheKey);

        T? r = default;
        if (data.Length <= 0)
        {
            if (semaphoreSlim is null ||
                (semaphoreSlim is not null && await semaphoreSlim.WaitAsync(semaphoreSlimWaitMsTimeout, cancellationToken)))
            {
                try
                {
                    // 二次检查
                    data = await database.StringGetAsync(cacheKey);
                    if (data.Length <= 0)
                    {
                        r = await getData(cancellationToken);

                        if (r is not null)
                        {
                            var serializeData = MemoryPackSerializer.Serialize(r);

                            if (expiryIsNull)
                            {
                                expiry = null;
                            }
                            else
                            {
                                expiry ??= TimeSpan.FromHours(1);
                            }
                            await database.StringSetAsync(cacheKey, serializeData, expiry);
                            return r;
                        }
                    }
                }
                catch { }
                finally
                {
                    semaphoreSlim?.Release();
                }
            }
            else // 获取锁失败直接获取缓存数据
            {
                data = await database.StringGetAsync(cacheKey);
            }
        }

        if (data.Length > 0)
        {
            r = MemoryPackSerializer.Deserialize<T>(data.Span);
        }

        return r;
    }

    /// <summary>
    /// 推送订单申请退款消息
    /// </summary>
    public static async Task PushOrderRefundRequestMessageAsync(
        IConnection rabbitmqConn,
        OrderRefundMessage message)
    {
        using var stream = m.GetStream();
        await JsonSerializer.SerializeAsync(stream, message,
            PaymentMinimalApisJsonSerializerContext.Default.OrderRefundMessage);
        var value = stream.GetBuffer().AsMemory()[..unchecked((int)stream.Length)];
        await ListRightPushAsync(rabbitmqConn, PaymentRefundRequest, value, CancellationToken.None);
    }

    /// <summary>
    /// 推送图片下载处理消息
    /// </summary>
    public static async Task PushImageHandleRequestMessageAsync(
        IConnection rabbitmqConn,
        ImageHandleRequestModel message,
        CancellationToken cancellationToken = default)
    {
        using var stream = m.GetStream();
        await JsonSerializer.SerializeAsync(stream, message,
            BasicMinimalApisJsonSerializerContext.Default.ImageHandleRequestModel);
        var value = stream.GetBuffer().AsMemory()[..unchecked((int)stream.Length)];
        await ListRightPushAsync(rabbitmqConn, ImageHandleRequest, value, CancellationToken.None);
        //using var channel = await rabbitmqConn.CreateChannelAsync(cancellationToken: cancellationToken);
        //await channel.BasicPublishAsync(COSQueueName, ImageHandleRequest, value, cancellationToken);
    }

    /// <summary>
    /// 推送【协议解约申请】通知
    /// </summary>
    public static async Task PushAgreementUnSignRequestMessageAsync(
        IConnection rabbitmqConn,
        string agreementNo)
    {
        var value = Encoding.UTF8.GetBytes(agreementNo);
        await ListRightPushAsync(rabbitmqConn, AgreementUnSignRequest, value, CancellationToken.None);
    }

    static readonly RecyclableMemoryStreamManager m = new();

    const string exchangeName = "amq.direct"; // 默认交换机

    public static async Task ListRightPushAsync(
        IConnection rabbitmqConn,
        string routingKey,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken = default)
    {
        using var channel = await rabbitmqConn.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.BasicPublishAsync(exchangeName, routingKey, body, cancellationToken);
    }
}