using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using WinDivertSharp;
using static AigioL.Common.Net.ReverseProxy.Infrastructure.PacketIntercept.LoggerMessages;

#pragma warning disable IDE0290 // 使用主构造函数
namespace AigioL.Common.Net.ReverseProxy.Infrastructure.PacketIntercept;

/// <summary>
/// <see cref="ITcpInterceptor"/> 的默认实现
/// </summary>
abstract class TcpInterceptor : ITcpInterceptor, IDisposable
{
    readonly string filter;
    readonly ushort oldServerPort;
    readonly ushort newServerPort;
    readonly ILogger logger;
    nint handle;
    bool disposedValue;

    public TcpInterceptor(ILogger logger, ushort oldServerPort, ushort newServerPort)
    {
        this.logger = logger;
        this.oldServerPort = oldServerPort;
        this.newServerPort = newServerPort;
        filter = $"loopback and (tcp.DstPort == {oldServerPort} or tcp.SrcPort == {newServerPort})";
    }

    /// <inheritdoc/>
    public async Task InterceptAsync(CancellationToken cancellationToken)
    {
        if (oldServerPort == newServerPort)
        {
            return;
        }

        var handle = WinDivert.WinDivertOpen(filter, WinDivertLayer.Network, 0, WinDivertOpenFlags.None);
        if (handle == WinDivert.INVALID_HANDLE_VALUE)
        {
            var lastError = Marshal.GetLastWin32Error();
            LogErrorWinDivertOpen(logger, (WinDivertOpenWin32ErrorCode)lastError);

            var win32Ex = new Win32Exception(lastError);
            throw win32Ex;
        }
        else
        {
            if (this.handle != default)
            {
                WinDivert.WinDivertClose(this.handle);
            }
            this.handle = handle;
        }

        if (Socket.OSSupportsIPv4)
        {
            LogInfoLoopbackIPv4(logger, oldServerPort, newServerPort);
        }
        if (Socket.OSSupportsIPv6)
        {
            LogInfoLoopbackIPv6(logger, oldServerPort, newServerPort);
        }

        var packetLength = 0U;
        using WinDivertBuffer winDivertBuffer = new();
        WinDivertAddress winDivertAddress = default;

        while (!cancellationToken.IsCancellationRequested)
        {
            winDivertAddress.Reset();
            if (WinDivert.WinDivertRecv(handle, winDivertBuffer, ref winDivertAddress, ref packetLength) == false)
            {
                var lastError = Marshal.GetLastWin32Error();
                LogErrorWinDivertRecv(logger, (WinDivertOpenWin32ErrorCode)lastError);

                var win32Ex = new Win32Exception(lastError);
                throw win32Ex;
            }

            try
            {
                ModifyTcpPacket(winDivertBuffer, ref winDivertAddress, ref packetLength);
            }
            catch (Exception ex)
            {
                LogErrorModifyTcpPacket(logger, ex);
            }
            finally
            {
                WinDivert.WinDivertSend(handle, winDivertBuffer, packetLength, ref winDivertAddress);
            }
        }
    }

    /// <summary>
    /// 修改 TCP 数据包
    /// </summary>
    unsafe void ModifyTcpPacket(WinDivertBuffer winDivertBuffer, ref WinDivertAddress winDivertAddress, ref uint packetLength)
    {
        var packet = WinDivert.WinDivertHelperParsePacket(winDivertBuffer, packetLength);
        if (packet.IPv4Header != null && packet.IPv4Header->SrcAddr.Equals(IPAddress.Loopback) == false)
        {
            return;
        }
        if (packet.IPv6Header != null && packet.IPv6Header->SrcAddr.Equals(IPAddress.IPv6Loopback) == false)
        {
            return;
        }

        if (packet.TcpHeader->DstPort == oldServerPort)
        {
            packet.TcpHeader->DstPort = newServerPort;
        }
        else
        {
            packet.TcpHeader->SrcPort = oldServerPort;
        }
        winDivertAddress.Impostor = true;
        WinDivert.WinDivertHelperCalcChecksums(winDivertBuffer, packetLength, ref winDivertAddress, WinDivertChecksumHelperParam.All);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                if (handle != default)
                {
                    WinDivert.WinDivertClose(handle);
                }
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            handle = default;
            disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

static partial class LoggerMessages
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "127.0.0.1:{oldServerPort} <=> 127.0.0.1:{newServerPort}")]
    internal static partial void LogInfoLoopbackIPv4(ILogger logger, ushort oldServerPort, ushort newServerPort);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "::1:{oldServerPort} <=> ::1:{newServerPort}")]
    internal static partial void LogInfoLoopbackIPv6(ILogger logger, ushort oldServerPort, ushort newServerPort);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "修改 TCP 数据包时发生错误")]
    internal static partial void LogErrorModifyTcpPacket(ILogger logger, Exception? exception);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "打开 WinDivert 时发生错误，Win32 错误代码: {win32ErrorCode}")]
    internal static partial void LogErrorWinDivertOpen(ILogger logger, WinDivertOpenWin32ErrorCode win32ErrorCode);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "接收 WinDivert 数据包时发生错误，Win32 错误代码: {win32ErrorCode}")]
    internal static partial void LogErrorWinDivertRecv(ILogger logger, WinDivertOpenWin32ErrorCode win32ErrorCode);
}