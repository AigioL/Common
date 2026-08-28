using System.Collections.Concurrent;
using System.Diagnostics;

namespace AigioL.Common.Net.ReverseProxy.Internals.Http;

/// <summary>
/// <see cref="LifetimeHttpHandler"/> 清理器
/// </summary>
sealed class LifetimeHttpHandlerCleaner
{
    /// <summary>
    /// 当前监视生命周期的记录的数量
    /// </summary>
    int trackingEntryCount = 0;

    /// <summary>
    /// 监视生命周期的记录队列
    /// </summary>
    readonly ConcurrentQueue<TrackingEntry> trackingEntries = new();

    /// <summary>
    /// 获取或设置清理的时间间隔，默认 10s
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromSeconds(10d);

    /// <summary>
    /// 添加要清除的 <see cref="LifetimeHttpHandler"/>
    /// </summary>
    public void Add(LifetimeHttpHandler handler)
    {
        var entry = new TrackingEntry(handler);
        trackingEntries.Enqueue(entry);

        // 从 0 变为 1，要启动清理作业
        if (Interlocked.Increment(ref trackingEntryCount) == 1)
        {
            StartCleanup();
        }
    }

    /// <summary>
    /// 启动清理作业
    /// </summary>
    async void StartCleanup()
    {
        await Task.Yield();
        while (Cleanup() == false)
        {
            await Task.Delay(CleanupInterval);
        }
    }

    /// <summary>
    /// 清理失效的拦截器，返回是否完全清理
    /// </summary>
    bool Cleanup()
    {
        var cleanCount = trackingEntries.Count;
        for (var i = 0; i < cleanCount; i++)
        {
            trackingEntries.TryDequeue(out var entry);
            Debug.Assert(entry != null);

            if (entry.CanDispose == false)
            {
                trackingEntries.Enqueue(entry);
                continue;
            }

            entry.Dispose();
            if (Interlocked.Decrement(ref trackingEntryCount) == 0)
            {
                return true;
            }
        }
        return false;
    }
}