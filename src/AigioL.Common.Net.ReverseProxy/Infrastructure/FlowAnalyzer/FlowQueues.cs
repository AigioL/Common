using System.Collections.Concurrent;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.FlowAnalyzer;

/// <summary>
/// 流量队列
/// </summary>
sealed class FlowQueues(int intervalSeconds)
{
    int cleaning = 0;
    long totalBytes = 0L;

    sealed record QueueItem(long Ticks, int Length);

    readonly ConcurrentQueue<QueueItem> queues = new();

    readonly int intervalSeconds = intervalSeconds;

    /// <summary>
    /// 总字节数
    /// </summary>
    public long TotalBytes => totalBytes;

    /// <summary>
    /// 收到数据
    /// </summary>
    public void OnFlow(int length)
    {
        Interlocked.Add(ref totalBytes, length);
        CleanInvalidRecords();
        queues.Enqueue(new QueueItem(Environment.TickCount64, length));
    }

    /// <summary>
    /// 获取速率
    /// </summary>
    public double GetRate()
    {
        CleanInvalidRecords();
        double intervalSecondsDouble = intervalSeconds;
        return queues.Sum(item => item.Length) / intervalSecondsDouble;
    }

    /// <summary>
    /// 清除无效记录
    /// </summary>
    bool CleanInvalidRecords()
    {
        if (Interlocked.CompareExchange(ref cleaning, 1, 0) != 0)
            return false;

        var ticks = Environment.TickCount64;
        while (queues.TryPeek(out var item))
        {
            if (ticks - item.Ticks < intervalSeconds * 1000)
            {
                break;
            }
            else
            {
                queues.TryDequeue(out _);
            }
        }

        Interlocked.Exchange(ref cleaning, 0);
        return true;
    }
}