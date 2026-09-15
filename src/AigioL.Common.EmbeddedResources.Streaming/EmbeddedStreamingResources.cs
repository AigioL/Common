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
    public static ((uint Start, uint End)[] Ranges, long Offset) Decompress(Stream resStream, RecyclableMemoryStream bufferStream)
    {
        using var decompressStream = new ZstandardStream(resStream, CompressionMode.Decompress);
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
    /// 压缩多个资源流，返回压缩后的流（数据总长度不得超过 <see cref="uint.MaxValue"/>，数量不得超过 <see cref="ushort.MaxValue"/>）
    /// </summary>
    public static void Compression(Stream[] streams, RecyclableMemoryStream bufferStream, CompressionLevel compressionLevel = CompressionLevel.SmallestSize)
    {
        bufferStream.Position = 0;
        try
        {
            using var compressionStream = new ZstandardStream(bufferStream, compressionLevel);
            WriteUShort(compressionStream, unchecked((ushort)streams.Length));

            uint calcPosition = 0;
            for (int i = 0; i < streams.Length; i++)
            {
                var stream = streams[i];
                var start = calcPosition;
                calcPosition += unchecked((uint)stream.Length);
                var end = calcPosition;
                WriteUInt(compressionStream, start);
                WriteUInt(compressionStream, end);
            }

            for (int i = 0; i < streams.Length; i++)
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
