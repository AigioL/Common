using Microsoft.IO;
using System.IO.Compression;
#if !NET11_0_OR_GREATER
using ZstandardStream = System.IO.Compression.BrotliStream;
#endif

namespace AigioL.Common.EmbeddedResources.Streaming;

/// <summary>
/// 嵌入流式资源的帮助类
/// </summary>
public static partial class EmbeddedStreamingResources
{
    /// <summary>
    /// 解压资源流，返回解压后的范围数组和偏移量
    /// </summary>
    [Obsolete("use Decompress2")]
    public static ((uint Start, uint End)[] Ranges, long Offset) Decompress(Stream resStream, RecyclableMemoryStream bufferStream, bool leaveOpen = false)
    {
        using var decompressStream = new ZstandardStream(resStream, CompressionMode.Decompress, leaveOpen: leaveOpen);
        decompressStream.CopyTo(bufferStream);
        bufferStream.Position = 0;

        var len = ReadUShort(bufferStream);
        var ranges = new ValueTuple<uint, uint>[len];
        for (int i = 0; i < len; i++)
        {
            ranges[i] = (ReadUInt(bufferStream), ReadUInt(bufferStream));
        }

        var offset = bufferStream.Position;

        return (ranges, offset);
    }

    /// <summary>
    /// 解压资源流，返回解压后的范围数组和偏移量
    /// </summary>
    public static IEnumerable<(uint Start, uint End, long Offset)> Decompress2(Stream resStream, RecyclableMemoryStream bufferStream, bool leaveOpen = false)
    {
        using var decompressStream = new ZstandardStream(resStream, CompressionMode.Decompress, leaveOpen: leaveOpen);
        decompressStream.CopyTo(bufferStream);
        bufferStream.Position = 0;

        var len = ReadUShort(bufferStream);
        var offset = sizeof(ushort) + (long)len * 2 * sizeof(uint);
        for (int i = 0; i < len; i++)
        {
            bufferStream.Position = sizeof(uint) * 2 * i + sizeof(ushort); // 修正当前读取的流位置，因 yield return 会导致外部调用者在迭代时改变流位置
            yield return (ReadUInt(bufferStream), ReadUInt(bufferStream), offset);
        }
    }

    public static Stream Slice(Stream source, (uint Start, uint End, long Offset) it, bool leaveOpen = false)
    {
        var start = it.Start + it.Offset;
        var end = it.End + it.Offset;
        return new SliceStream(source, start, end - start, leaveOpen);
    }

    public static Stream Slice(Stream source, long offset, long length, bool leaveOpen = false)
    {
        return new SliceStream(source, offset, length, leaveOpen);
    }

    /// <summary>
    /// 压缩多个资源流，返回压缩后的流（数据总长度不得超过 <see cref="uint.MaxValue"/>，数量不得超过 <see cref="ushort.MaxValue"/>）
    /// </summary>
    public static void Compression(IReadOnlyList<Stream> streams, RecyclableMemoryStream bufferStream, CompressionLevel compressionLevel = CompressionLevel.SmallestSize)
    {
        bufferStream.Position = 0;
        try
        {
            using var compressionStream = new ZstandardStream(bufferStream, compressionLevel, leaveOpen: true);
            WriteUShort(compressionStream, unchecked((ushort)streams.Count));

            uint calcPosition = 0;
            for (int i = 0; i < streams.Count; i++)
            {
                var stream = streams[i];
                var start = calcPosition;
                calcPosition += unchecked((uint)stream.Length);
                var end = calcPosition;
                WriteUInt(compressionStream, start);
                WriteUInt(compressionStream, end);
            }

            for (int i = 0; i < streams.Count; i++)
            {
                var stream = streams[i];
                stream.Position = 0;
                stream.CopyTo(compressionStream);
            }
        }
        finally
        {
            bufferStream.Position = 0;
        }
    }

    static ushort ReadUShort(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        stream.ReadExactly(buffer);
        return BitConverter.ToUInt16(buffer);
    }

    static void WriteUShort(Stream stream, ushort value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        BitConverter.TryWriteBytes(buffer, value);
        stream.Write(buffer);
    }

    static uint ReadUInt(Stream stream)
    {
        Span<byte> buffer = stackalloc byte[sizeof(uint)];
        stream.ReadExactly(buffer);
        return BitConverter.ToUInt32(buffer);
    }

    static void WriteUInt(Stream stream, uint value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(uint)];
        BitConverter.TryWriteBytes(buffer, value);
        stream.Write(buffer);
    }
}

file sealed partial class SliceStream : Stream
{
    readonly Stream source;

    readonly long start;

    readonly long length;

    readonly bool leaveOpen;

    long position;

    public SliceStream(Stream source, long offset, long length, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!source.CanRead || !source.CanSeek)
            throw new ArgumentException("源流必须支持读取和定位。", nameof(source));

        if (offset < 0 || length < 0 || offset > source.Length - length)
            throw new ArgumentOutOfRangeException(nameof(length));

        this.source = source;
        start = offset;
        this.length = length;
        this.leaveOpen = leaveOpen;
    }

    public override bool CanRead => source.CanRead;

    public override bool CanSeek => source.CanSeek;

    public override bool CanWrite => false;

    public override long Length => length;

    public override long Position
    {
        get => position;
        set
        {
            if (value < 0 || value > length)
                throw new ArgumentOutOfRangeException(nameof(value));

            position = value;
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return Read(buffer.AsSpan(offset, count));
    }

    public override int Read(Span<byte> buffer)
    {
        int readLength = (int)Math.Min(buffer.Length, length - position);
        if (readLength == 0)
            return 0;

        source.Position = start + position;
        int read = source.Read(buffer[..readLength]);
        position += read;

        return read;
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        int readLength = (int)Math.Min(buffer.Length, length - position);
        if (readLength == 0)
            return 0;

        source.Position = start + position;
        int read = await source.ReadAsync(
            buffer[..readLength],
            cancellationToken);

        position += read;
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        long newPosition = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => position + offset,
            SeekOrigin.End => length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };

        Position = newPosition;
        return position;
    }

    public override void Flush() { }

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing && !leaveOpen)
            source.Dispose();

        base.Dispose(disposing);
    }
}