/*
 * WinDivertNative.cs
 * (C) 2018, all rights reserved,
 *
 * This file is part of WinDivertSharp.
 *
 * WinDivertSharp is free software: you can redistribute it and/or modify it under
 * the terms of the GNU Lesser General Public License as published by the
 * Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public
 * License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * WinDivertSharp is free software; you can redistribute it and/or modify it under
 * the terms of the GNU General Public License as published by the Free
 * Software Foundation; either version 2 of the License, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
 * for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc., 51
 * Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.
 */

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
#if !NET35
using PathCompat = System.IO.Path;
#endif

namespace WinDivertSharp;

internal static unsafe class WinDivertNative
{
    const string WinDivert = "WinDivert.dll";

    /// <summary>
    /// Open a WinDivert handle.
    /// </summary>
    /// <param name="filter">Filter string.</param>
    /// <param name="layer">Packet processing layer.</param>
    /// <param name="priority">Handle priority.</param>
    /// <param name="flags">Open flags.</param>
    /// <returns>Handle to the WinDivert instance, or <see cref="nint.Zero"/> on failure.</returns>
    [DllImport(WinDivert, EntryPoint = "WinDivertOpen", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern nint WinDivertOpen([In][MarshalAs(UnmanagedType.LPStr)] string filter, WinDivertLayer layer, short priority, ulong flags);

    /// <summary>
    /// Receive (read) a packet from a WinDivert handle.
    /// </summary>
    /// <param name="handle">WinDivert handle.</param>
    /// <param name="pPacket">Buffer that receives the packet.</param>
    /// <param name="packetLen">Size of <paramref name="pPacket"/> in bytes.</param>
    /// <param name="pAddr">Receives packet metadata.</param>
    /// <param name="readLen">Receives number of bytes read.</param>
    /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
    [DllImport(WinDivert, EntryPoint = "WinDivertRecv", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WinDivertRecv([In] nint handle, nint pPacket, uint packetLen, [In] ref WinDivertAddress pAddr, ref uint readLen);

    /// <summary>
    /// Receive (read) a packet from a WinDivert handle.
    /// </summary>
    /// <param name="handle">WinDivert handle.</param>
    /// <param name="pPacket">Buffer that receives the packet.</param>
    /// <param name="packetLen">Size of <paramref name="pPacket"/> in bytes.</param>
    /// <param name="flags">Receive flags.</param>
    /// <param name="pAddr">Receives packet metadata.</param>
    /// <param name="readLen">Receives number of bytes read.</param>
    /// <param name="lpOverlapped">Overlapped I/O structure for asynchronous operations.</param>
    /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
    [DllImport(WinDivert, EntryPoint = "WinDivertRecvEx", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WinDivertRecvEx([In] nint handle, nint pPacket, uint packetLen, ulong flags, ref WinDivertAddress pAddr, ref uint readLen, ref NativeOverlapped lpOverlapped);

    /// <summary>
    /// Send (write/inject) a packet to a WinDivert handle.
    /// </summary>
    /// <param name="handle">WinDivert handle.</param>
    /// <param name="pPacket">Buffer containing the packet to send.</param>
    /// <param name="packetLen">Packet length in bytes.</param>
    /// <param name="pAddr">Packet metadata.</param>
    /// <param name="writeLen">Receives number of bytes written.</param>
    /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
    [DllImport(WinDivert, EntryPoint = "WinDivertSend", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WinDivertSend([In] nint handle, [In] nint pPacket, uint packetLen, [In] ref WinDivertAddress pAddr, ref uint writeLen);

    /// <summary>
    /// Send (write/inject) a packet to a WinDivert handle.
    /// </summary>
    /// <param name="handle">WinDivert handle.</param>
    /// <param name="pPacket">Buffer containing the packet to send.</param>
    /// <param name="packetLen">Packet length in bytes.</param>
    /// <param name="flags">Send flags.</param>
    /// <param name="pAddr">Packet metadata.</param>
    /// <param name="writeLen">Receives number of bytes written.</param>
    /// <param name="lpOverlapped">Overlapped I/O structure for asynchronous operations.</param>
    /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
    [DllImport(WinDivert, EntryPoint = "WinDivertSendEx", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WinDivertSendEx([In] nint handle, [In] nint pPacket, uint packetLen, ulong flags, [In] ref WinDivertAddress pAddr, ref uint writeLen, ref NativeOverlapped lpOverlapped);

    /// <summary>
    /// Send (write/inject) a packet to a WinDivert handle.
    /// </summary>
    /// <param name="handle">WinDivert handle.</param>
    /// <param name="pPacket">Buffer containing the packet to send.</param>
    /// <param name="packetLen">Packet length in bytes.</param>
    /// <param name="flags">Send flags.</param>
    /// <param name="pAddr">Packet metadata.</param>
    /// <param name="ignoredLenPtr">Ignored optional write length pointer.</param>
    /// <param name="ignoredOverlappedPtr">Ignored optional overlapped pointer.</param>
    /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
    [DllImport(WinDivert, EntryPoint = "WinDivertSendEx", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WinDivertSendEx([In] nint handle, [In] nint pPacket, uint packetLen, ulong flags, [In] ref WinDivertAddress pAddr, nint ignoredLenPtr, nint ignoredOverlappedPtr);

    /// <summary>
    /// Close a WinDivert handle.
    /// </summary>
    /// <param name="handle">WinDivert handle.</param>
    /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
    [DllImport(WinDivert, EntryPoint = "WinDivertClose", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WinDivertClose([In] nint handle);

    /// <summary>
    /// Set a WinDivert handle parameter.
    /// </summary>
    /// <param name="handle">WinDivert handle.</param>
    /// <param name="param">Parameter to set.</param>
    /// <param name="value">Parameter value.</param>
    /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
    [DllImport(WinDivert, EntryPoint = "WinDivertSetParam", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WinDivertSetParam([In] nint handle, WinDivertParam param, ulong value);

    /// <summary>
    /// Get a WinDivert handle parameter.
    /// </summary>
    /// <param name="handle">WinDivert handle.</param>
    /// <param name="param">Parameter to query.</param>
    /// <param name="pValue">Receives parameter value.</param>
    /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
    [DllImport(WinDivert, EntryPoint = "WinDivertGetParam", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WinDivertGetParam([In] nint handle, WinDivertParam param, [Out] out ulong pValue);

    /// <summary>
    /// Parse IPv4/IPv6/ICMP/ICMPv6/TCP/UDP headers from a raw packet.
    /// </summary>
    /// <param name="pPacket">Packet buffer.</param>
    /// <param name="packetLen">Packet length in bytes.</param>
    /// <param name="ppIpHdr">Receives IPv4 header pointer.</param>
    /// <param name="ppIpv6Hdr">Receives IPv6 header pointer.</param>
    /// <param name="ppIcmpHdr">Receives ICMP header pointer.</param>
    /// <param name="ppIcmpv6Hdr">Receives ICMPv6 header pointer.</param>
    /// <param name="ppTcpHdr">Receives TCP header pointer.</param>
    /// <param name="ppUdpHdr">Receives UDP header pointer.</param>
    /// <param name="ppData">Receives payload data pointer.</param>
    /// <param name="pDataLen">Receives payload data length.</param>
    /// <returns><see langword="true"/> on success; otherwise <see langword="false"/>.</returns>
    [DllImport(WinDivert, EntryPoint = "WinDivertHelperParsePacket", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WinDivertHelperParsePacket([In] nint pPacket, uint packetLen, IPv4Header** ppIpHdr, IPv6Header** ppIpv6Hdr, IcmpV4Header** ppIcmpHdr, IcmpV6Header** ppIcmpv6Hdr, TcpHeader** ppTcpHdr, UdpHeader** ppUdpHdr, byte** ppData, ref uint pDataLen);

    /// <summary>
    /// Calculate IPv4/IPv6/ICMP/ICMPv6/TCP/UDP checksums.
    /// </summary>
    /// <param name="pPacket">Packet buffer.</param>
    /// <param name="packetLen">Packet length in bytes.</param>
    /// <param name="pAddr">Packet metadata.</param>
    /// <param name="flags">Checksum calculation flags.</param>
    /// <returns>The number of checksums recalculated.</returns>
    [DllImport(WinDivert, EntryPoint = "WinDivertHelperCalcChecksums", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern uint WinDivertHelperCalcChecksums(nint pPacket, uint packetLen, [In] ref WinDivertAddress pAddr, ulong flags);

    /// <summary>
    /// Calculate IPv4/IPv6/ICMP/ICMPv6/TCP/UDP checksums.
    /// </summary>
    /// <param name="pPacket">Packet buffer.</param>
    /// <param name="packetLen">Packet length in bytes.</param>
    /// <param name="ignoredAddress">Ignored optional packet metadata pointer.</param>
    /// <param name="flags">Checksum calculation flags.</param>
    /// <returns>The number of checksums recalculated.</returns>
    [DllImport(WinDivert, EntryPoint = "WinDivertHelperCalcChecksums", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern uint WinDivertHelperCalcChecksums(nint pPacket, uint packetLen, [In] nint ignoredAddress, ulong flags);

    /// <summary>
    /// Calculate IPv4/IPv6/ICMP/ICMPv6/TCP/UDP checksums.
    /// </summary>
    /// <param name="pPacket">Packet buffer.</param>
    /// <param name="packetLen">Packet length in bytes.</param>
    /// <param name="ignoredAddress">Ignored optional packet metadata pointer.</param>
    /// <param name="flags">Checksum calculation flags.</param>
    /// <returns>The number of checksums recalculated.</returns>
    [DllImport(WinDivert, EntryPoint = "WinDivertHelperCalcChecksums", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern uint WinDivertHelperCalcChecksums(byte* pPacket, uint packetLen, [In] nint ignoredAddress, ulong flags);

    /// <summary>
    /// Check the given filter string.
    /// </summary>
    /// <param name="filter">Filter string.</param>
    /// <param name="layer">Packet processing layer.</param>
    /// <param name="errorStr">Receives pointer to an error message on failure.</param>
    /// <param name="errorPos">Receives error position in the filter string.</param>
    /// <returns><see langword="true"/> if the filter is valid; otherwise <see langword="false"/>.</returns>
    [DllImport(WinDivert, EntryPoint = "WinDivertHelperCheckFilter", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WinDivertHelperCheckFilter([In][MarshalAs(UnmanagedType.LPStr)] string filter, WinDivertLayer layer, char** errorStr, ref uint errorPos);

    /// <summary>
    /// Evaluate the given filter string.
    /// </summary>
    /// <param name="filter">Filter string.</param>
    /// <param name="layer">Packet processing layer.</param>
    /// <param name="pPacket">Packet buffer to evaluate.</param>
    /// <param name="packetLen">Packet length in bytes.</param>
    /// <param name="pAddr">Packet metadata.</param>
    /// <returns><see langword="true"/> if the packet matches the filter; otherwise <see langword="false"/>.</returns>
    [DllImport(WinDivert, EntryPoint = "WinDivertHelperEvalFilter", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WinDivertHelperEvalFilter([In][MarshalAs(UnmanagedType.LPStr)] string filter, WinDivertLayer layer, [In] nint pPacket, uint packetLen, [In] ref WinDivertAddress pAddr);
}