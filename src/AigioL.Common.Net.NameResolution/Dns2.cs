using AigioL.Common.Net.NameResolution.Models;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AigioL.Common.Net.NameResolution;

/// <inheritdoc cref="Dns"/>
public static partial class Dns2
{
    /// <summary>
    /// 异步解析指定主机名的指定地址族（使用系统默认 DNS 服务器）
    /// </summary>
    /// <param name="name">要解析的主机名</param>
    /// <param name="addressFamily">要查询的地址族。使用 <see cref="AddressFamily.InterNetwork"/> 表示 A 记录，<see cref="AddressFamily.InterNetworkV6"/> 表示 AAAA 记录，<see cref="AddressFamily.Unspecified"/> 表示两者兼有</param>
    /// <param name="cancellationToken">用于取消异步操作的令牌</param>
    /// <returns>一个任务，完成后会生成一个包含地址记录的 <see cref="DnsResult{T}"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<DnsResultWrapper<AddressRecord>> ResolveAddressesAsync(string name, AddressFamily addressFamily, CancellationToken cancellationToken = default)
    {
        var timestamp = Stopwatch.GetTimestamp();
        try
        {
#if NET11_0_OR_GREATER
            // https://github.com/dotnet/runtime/blob/v11.0.0-preview.7.26381.103/src/libraries/System.Net.NameResolution/src/System/Net/Dns.Resolve.cs#L51
            var r = await Dns.ResolveAddressesAsync(name, addressFamily, cancellationToken);
            return r.ToWrapper(DnsResultSourceType.SystemDefault, elapsedTime: Stopwatch.GetElapsedTime(timestamp));
#else
#endif
        }
        catch (PlatformNotSupportedException)
        {
            // https://github.com/dotnet/runtime/blob/v11.0.0-preview.7.26381.103/src/libraries/System.Net.NameResolution/src/System/Net/DnsResolverPal.Unsupported.cs#L17
        }
        catch
        {
            throw;
        }

        var addresses = await Dns.GetHostAddressesAsync(name, addressFamily, cancellationToken);
        if (addresses != null && addresses.Length != 0)
        {
            var r = new Dns2Result<AddressRecord>(DnsResponseCode.NoError, [.. addresses.Select(static it =>
            {
                AddressRecord2 r = new(it, default);
                AddressRecord r2 = r;
                return r2;
            })]);
            return r.ToWrapper(DnsResultSourceType.SystemDefault, elapsedTime: Stopwatch.GetElapsedTime(timestamp));
        }
        return DnsResponseCode.ServerFailure;
    }

    [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
#pragma warning disable SYSLIB1054 // 使用 “LibraryImportAttribute” 而不是 “DllImportAttribute” 在编译时生成 P/Invoke 封送代码
    static extern void DnsFlushResolverCache();
#pragma warning restore SYSLIB1054 // 使用 “LibraryImportAttribute” 而不是 “DllImportAttribute” 在编译时生成 P/Invoke 封送代码

    /// <summary>
    /// 刷新 DNS 缓存
    /// </summary>
    public static void FlushResolverCache()
    {
#if !WINDOWS
        if (OperatingSystem.IsWindows())
#endif
        {
            try
            {
                DnsFlushResolverCache();
            }
            catch
            {
            }
        }
    }
}
