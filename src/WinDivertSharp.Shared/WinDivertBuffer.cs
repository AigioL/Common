/*
 * WinDivertBuffer.cs
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
using System.Runtime.InteropServices;

namespace WinDivertSharp;

/// <summary>
/// Represents a buffer to be used for sending, receiving and manipulating packets.
/// </summary>
/// <remarks>
/// This buffer must remain intact for as long as the user retains any structures generated from
/// it. The purpose of this object is to safely pin the buffer data inside. Nearly all other
/// classes constructed in the use of this library will be created with unsafe pointer-based
/// references that all require their source (the buffer) to be pinned in memory.
/// </remarks>
public sealed class WinDivertBuffer : IDisposable
{
    /// <summary>
    /// The internal buffer object.
    /// </summary>
    byte[] _buffer;

    /// <summary>
    /// The pinned pointer to the buffer.
    /// </summary>
    internal IntPtr BufferPointer;

    /// <summary>
    /// The GCHandle that provides our <see cref="BufferPointer"/> member.
    /// </summary>
    GCHandle _bufferHandle;

    public const int DefaultMaxPacketSize = 65536;

    /// <summary>
    /// Constructs a new buffer with the default max-packet size.
    /// </summary>
    public WinDivertBuffer() : this(DefaultMaxPacketSize)
    {
    }

    /// <summary>
    /// Constructs a new buffer from the given raw buffer data.
    /// </summary>
    /// <param name="bufferData">
    /// The raw buffer data to wrap.
    /// </param>
    public WinDivertBuffer(byte[] bufferData)
    {
        _buffer = bufferData;
        _bufferHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
        BufferPointer = _bufferHandle.AddrOfPinnedObject();
    }

    /// <summary>
    /// Constructs a new buffer with the given size.
    /// </summary>
    /// <param name="bufferSize"></param>
    public WinDivertBuffer(int bufferSize)
    {
        _buffer = new byte[bufferSize];
        _bufferHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
        BufferPointer = _bufferHandle.AddrOfPinnedObject();
    }

    /// <summary>
    /// Gets or sets the buffer value at the specified index.
    /// </summary>
    /// <param name="index">
    /// The index.
    /// </param>
    /// <exception cref="IndexOutOfRangeException">
    /// Will throw if the supplied index is out of range.
    /// </exception>
    public byte this[int index]
    {
        get
        {
            return _buffer[index];
        }

        set
        {
            _buffer[index] = value;
        }
    }

    /// <summary>
    /// Gets or sets the buffer value at the specified index.
    /// </summary>
    /// <param name="index">
    /// The index.
    /// </param>
    /// <exception cref="IndexOutOfRangeException">
    /// Will throw if the supplied index is out of range.
    /// </exception>
    public byte this[uint index]
    {
        get
        {
            return _buffer[index];
        }

        set
        {
            _buffer[index] = value;
        }
    }

    /// <summary>
    /// Gets the length of the buffer.
    /// </summary>
    public uint Length
    {
        get
        {
            return (uint)_buffer.Length;
        }
    }

    #region IDisposable Support

    bool disposedValue; // To detect redundant calls

    /// <summary>
    /// Disposes of the buffer.
    /// </summary>
    /// <param name="disposing">
    /// Whether or not we're disposing.
    /// </param>
    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                if (_buffer != null)
                {
                    _bufferHandle.Free();
                    BufferPointer = IntPtr.Zero;
                    Array.Clear(_buffer, 0, _buffer.Length);
                }
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            _buffer = null!;

            disposedValue = true;
        }
    }

    /// <summary>
    /// Disposes the buffer.
    /// </summary>
    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Support
}