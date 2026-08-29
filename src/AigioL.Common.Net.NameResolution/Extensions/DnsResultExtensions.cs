using AigioL.Common.Net.NameResolution.Models;
using System.Net;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.Net.NameResolution;

public static partial class DnsResultExtensions
{
    public static DnsResultWrapper<T> ToWrapper<T>(this Dns2Result<T> result, DnsResultSourceType sourceType, string? traceId = null, TimeSpan? elapsedTime = null) => new(sourceType, result, traceId, elapsedTime);

    public static DnsResultWrapper<T> ToWrapper<T>(this DnsResult<T> result, DnsResultSourceType sourceType, string? traceId = null, TimeSpan? elapsedTime = null) => new(sourceType, result, traceId, elapsedTime);

    public static DnsResultWrapper<AddressRecord> MergeAddressResults(this DnsResultWrapper<AddressRecord> a, DnsResultWrapper<AddressRecord> b)
    {
        if (a.Result.Records.Count > 0 || b.Result.Records.Count > 0)
        {
            AddressRecord[] merged = [.. a.Result.Records, .. b.Result.Records];
            // A positive result carries no negative-cache TTL.
            return new Dns2Result<AddressRecord>(DnsResponseCode.NoError, merged, TimeSpan.Zero)
                .ToWrapper(a.SourceType, a.TraceId ?? b.TraceId);
        }

        DnsResponseCode chosenRc = a.Result.ResponseCode == DnsResponseCode.NxDomain || b.Result.ResponseCode == DnsResponseCode.NxDomain
            ? DnsResponseCode.NxDomain
            : (a.Result.ResponseCode != DnsResponseCode.NoError ? a.Result.ResponseCode : b.Result.ResponseCode);
        TimeSpan negTtl = MinNonZero(a.Result.NegativeCacheTtl, b.Result.NegativeCacheTtl);
        return new Dns2Result<AddressRecord>(chosenRc, null, negTtl)
            .ToWrapper(a.SourceType, a.TraceId ?? b.TraceId);
    }

    static TimeSpan MinNonZero(TimeSpan x, TimeSpan y)
    {
        if (x <= TimeSpan.Zero)
        {
            return y > TimeSpan.Zero ? y : TimeSpan.Zero;
        }

        if (y <= TimeSpan.Zero)
        {
            return x;
        }

        return x < y ? x : y;
    }

    /// <summary>
    /// 并行执行任意 <see cref="Task"/>，成功返回
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ts">一组异步任务</param>
    /// <param name="predicate">判断返回值是否成功的委托</param>
    /// <param name="cts">取消令牌源</param>
    public static async Task<DnsResultWrapper<T>> ParallelWhenAnyAsync<T>(
        this List<Task<DnsResultWrapper<T>>> ts,
        Func<DnsResult<T>, bool> predicate,
        CancellationTokenSource cts)
    {
        try
        {
            while (ts.Count != 0)
            {
                var t = await Task.WhenAny(ts);
                var r = t.Result;
                if (predicate(r.Result))
                {
                    // 有成功解析出的值，返回
                    cts.Cancel();
                    return r;
                }
                ts.Remove(t);
            }
            // 遍历循环完成，没有符合条件的成功，返回失败
            return DnsResponseCode.ServerFailure;
        }
        finally
        {
            cts.Cancel();
        }
    }

    public static bool HasValue(DnsResult<AddressRecord> r)
    {
        if (r.ResponseCode == DnsResponseCode.NoError)
        {
            if (r.Records != null && r.Records.Any(x => x.Address != null))
            {
                // 有成功解析出的值，返回
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获取该结果的绝对过期时间（相对于当前时间的时间间隔），如果是负缓存结果，则返回负缓存 TTL
    /// </summary>
    public static TimeSpan GetAbsoluteExpirationRelativeToNow(this DnsResult<AddressRecord> r)
    {
        if (r.ResponseCode == DnsResponseCode.NoError)
        {
            if (r.Records != null && r.Records.Any(x => x.Address != null))
            {
                return r.Records.Min(static x => x.Ttl);
            }
        }
        else
        {
            if (r.NegativeCacheTtl > TimeSpan.Zero && r.NegativeCacheTtl <= TimeSpan.FromMinutes(5))
            {
                // RFC 2308 https://datatracker.ietf.org/doc/html/rfc2308
                // 7.1 服务器故障（可选）
                // 7.2 失效/无法访问服务器（可选）
                // 在任何一种情况下，解析程序都可能缓存服务器故障响应。如果这样做，则缓存时间不得超过五（5）分钟
                return r.NegativeCacheTtl;
            }
        }
        return default;
    }

    /// <summary>
    /// 多个 DNS 实现返回的结果可能包含重复的 IP 地址，将 IP 地址去重后返回
    /// </summary>
    public static DnsResultWrapper<IPAddress> Distinct(this DnsResultWrapper<AddressRecord> r)
    {
        var addresses = r.Result.Records.Select(static x => x.Address).Distinct().ToArray();
        return new DnsResultWrapper<IPAddress>(r.SourceType, new Dns2Result<IPAddress>(r.Result.ResponseCode, addresses, r.Result.NegativeCacheTtl), r.TraceId, r.ElapsedTime);
    }
}
