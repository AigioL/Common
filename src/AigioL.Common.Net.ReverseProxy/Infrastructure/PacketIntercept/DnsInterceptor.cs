using AigioL.Common.Net.NameResolution;
using AigioL.Common.Net.ReverseProxy.Infrastructure.Configuration;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.InteropServices;
using WinDivertSharp;
using static AigioL.Common.Net.ReverseProxy.Infrastructure.PacketIntercept.LoggerMessages;

#pragma warning disable IDE0290 // 使用主构造函数
namespace AigioL.Common.Net.ReverseProxy.Infrastructure.PacketIntercept;

sealed partial class DnsInterceptor : IDnsInterceptor, IDisposable
{
    const string DNS_FILTER = "udp.DstPort == 53";
    readonly ILogger logger;
    readonly IReverseProxyConfig reverseProxyConfig;
    readonly TimeSpan ttl = TimeSpan.FromMinutes(5d);
    nint handle;
    bool disposedValue;

    public DnsInterceptor(ILogger<DnsInterceptor> logger, IReverseProxyConfig reverseProxyConfig)
    {
        this.logger = logger;
        this.reverseProxyConfig = reverseProxyConfig;
    }

    /// <inheritdoc/>
    public async Task InterceptAsync(CancellationToken cancellationToken)
    {
        var handle = WinDivert.WinDivertOpen(DNS_FILTER, WinDivertLayer.Network, 0, WinDivertOpenFlags.None);
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

        var packetLength = 0U;
        using WinDivertBuffer winDivertBuffer = new();
        WinDivertAddress winDivertAddress = default;

        Dns2.FlushResolverCache();
        while (!cancellationToken.IsCancellationRequested)
        {
            if (WinDivert.WinDivertRecv(handle, winDivertBuffer, ref winDivertAddress, ref packetLength) == false)
            {
                var lastError = Marshal.GetLastWin32Error();
                LogErrorWinDivertRecv(logger, (WinDivertOpenWin32ErrorCode)lastError);

                var win32Ex = new Win32Exception(lastError);
                throw win32Ex;
            }

            try
            {
                ModifyDnsPacket(winDivertBuffer, ref winDivertAddress, ref packetLength);
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
    /// 修改 DNS 数据包
    /// </summary>
    unsafe void ModifyDnsPacket(WinDivertBuffer winDivertBuffer, ref WinDivertAddress winDivertAddress, ref uint packetLength)
    {
        var packet = WinDivert.WinDivertHelperParsePacket(winDivertBuffer, packetLength);
        Debug.Assert(packet.PacketPayloadLength <= WinDivertBuffer.DefaultMaxPacketSize,
            "DNS 请求长度超出 DefaultMaxPacketSize，可能会导致内存访问越界！");
        var requestPayload = new Span<byte>(packet.PacketPayload, unchecked((int)packet.PacketPayloadLength));

        if (!TryParseRequest(requestPayload, out var request) ||
            request.OperationCode != OperationCode.Query ||
            request.Questions.Count == 0)
        {
            return;
        }

        var question = request.Questions.First();
        if (question.Type != RecordType.A && question.Type != RecordType.AAAA)
        {
            return;
        }

        var domain = question.Name;
        if (!reverseProxyConfig.IsMatch(question.Name.ToString()))
        {
            return;
        }

        // DNS 响应数据
        var response = Response.FromRequest(request);
        var loopback = question.Type == RecordType.A ? IPAddress.Loopback : IPAddress.IPv6Loopback;
        var record = new IPAddressResourceRecord(domain, loopback, ttl);
        response.AnswerRecords.Add(record);
        var responseSize = response.Size;
        Debug.Assert(responseSize <= WinDivertBuffer.DefaultMaxPacketSize,
            "DNS 响应长度超出 DefaultMaxPacketSize，可能会导致内存访问越界！");
        var responsePayload = new Span<byte>(packet.PacketPayload, responseSize);

        // 修改 payload 和包长 
        response.Write(responsePayload);
        packetLength = (uint)((int)packetLength + responsePayload.Length - requestPayload.Length);

        // 修改 IP 包
        IPAddress destAddress;
        if (packet.IPv4Header != null)
        {
            destAddress = packet.IPv4Header->DstAddr;
            packet.IPv4Header->DstAddr = packet.IPv4Header->SrcAddr;
            packet.IPv4Header->SrcAddr = destAddress;
            packet.IPv4Header->Length = (ushort)packetLength;
        }
        else
        {
            destAddress = packet.IPv6Header->DstAddr;
            packet.IPv6Header->DstAddr = packet.IPv6Header->SrcAddr;
            packet.IPv6Header->SrcAddr = destAddress;
            packet.IPv6Header->Length = (ushort)(packetLength - sizeof(IPv6Header));
        }

        // 修改 UDP 包
        (packet.UdpHeader->SrcPort, packet.UdpHeader->DstPort) = (packet.UdpHeader->DstPort, packet.UdpHeader->SrcPort);
        packet.UdpHeader->Length = (ushort)(sizeof(UdpHeader) + responsePayload.Length);

        winDivertAddress.Impostor = true;
        winDivertAddress.Direction = winDivertAddress.Loopback
            ? WinDivertDirection.Outbound
            : WinDivertDirection.Inbound;

        WinDivert.WinDivertHelperCalcChecksums(winDivertBuffer, packetLength, ref winDivertAddress, WinDivertChecksumHelperParam.All);

        LogInfoDomainToLoopback(logger, domain, question.Type); // 记录日志，域名已解析到本地 localhost
    }

    static bool TryParseRequest(ReadOnlySpan<byte> payload, [NotNullWhen(true)] out Request? request)
    {
        try
        {
            request = Request.FromArray(payload);
            return true;
        }
        catch (Exception)
        {
            request = null;
            return false;
        }
    }

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                if (handle != default)
                {
                    WinDivert.WinDivertClose(handle);
                    Dns2.FlushResolverCache();
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
        Message = "{domain} -> localhost({recordType})")]
    internal static partial void LogInfoDomainToLoopback(ILogger logger, Domain? domain, RecordType recordType);
}