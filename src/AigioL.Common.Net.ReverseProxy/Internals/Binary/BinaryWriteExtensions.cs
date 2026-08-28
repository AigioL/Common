using System.Buffers.Text;
using System.Text;
using Utf8StringInterpolation;
using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Buffers;

static partial class BinaryWriteExtensions
{
    /// <summary>
    /// 将字节使用 Base64 编码写入缓冲区
    /// </summary>
    public static void AppendBase64String<TBufferWriter>(
        this Utf8StringWriter<TBufferWriter> writer,
        ReadOnlySpan<byte> value)
        where TBufferWriter : IBufferWriter<byte>
    {
        char[]? chars = null;
        try
        {
            var len = Base64.GetEncodedLength(value.Length);
            Span<char> authBasic = len <= StackallocCharThreshold ?
                stackalloc char[StackallocCharThreshold] :
                (chars = ArrayPool<char>.Shared.Rent(len));
            var d = Convert.TryToBase64Chars(value, authBasic, out var charsWritten);
            if (d)
            {
                authBasic = authBasic[..charsWritten];
                writer.AppendFormatted(authBasic);
            }
            else
            {
                writer.Append(Convert.ToBase64String(value));
            }
        }
        finally
        {
            if (chars is not null)
            {
                ArrayPool<char>.Shared.Return(chars);
            }
        }
    }

    /// <summary>
    /// 将字符串以 UTF-8 编码写入流
    /// </summary>
    public static void WriteUtf8(this Stream stream, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        stream.WriteUtf8(value.AsSpan());
    }

    /// <summary>
    /// 将字符 Span 以 UTF-8 编码写入流
    /// </summary>
    public static void WriteUtf8(this Stream stream, ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
        {
            return;
        }

        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(value.Length);

        byte[]? array = null;
        Span<byte> utf8Bytes = expectedByteCount <= StackallocByteThreshold ?
            stackalloc byte[StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount));

        try
        {
            var actualByteCount = Encoding.UTF8.GetBytes(value, utf8Bytes);
            utf8Bytes = utf8Bytes[..actualByteCount];
            stream.Write(utf8Bytes);
        }
        finally
        {
            if (array is not null)
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
    }

    /// <summary>
    /// 将字符串以 UTF-8 编码写入流，并在头部写入 <see langword="byte"/> 格式的长度，当内容长度超过 <see cref="byte.MaxValue"/>，将抛出 argumentOutOfRangeEx
    /// </summary>
    public static void WriteUtf8WithLength(this Stream stream, string? value, Exception argumentOutOfRangeEx)
    {
        if (string.IsNullOrEmpty(value))
        {
            stream.WriteByte(0);
            return;
        }

        stream.WriteUtf8WithLength(value.AsSpan(), argumentOutOfRangeEx);
    }

    /// <summary>
    /// 将字符 Span 以 UTF-8 编码写入流，并在头部写入 <see langword="byte"/> 格式的长度，当内容长度超过 <see cref="byte.MaxValue"/>，将抛出 argumentOutOfRangeEx
    /// </summary>
    public static void WriteUtf8WithLength(this Stream stream, ReadOnlySpan<char> value, Exception argumentOutOfRangeEx)
    {
        if (value.IsEmpty)
        {
            stream.WriteByte(0);
            return;
        }

        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(value.Length);
        if (expectedByteCount > byte.MaxValue)
        {
            throw argumentOutOfRangeEx;
        }
        // 写入长度，最大值不能超过 255
        stream.WriteByte(unchecked((byte)expectedByteCount));

        byte[]? array = null;
        Span<byte> utf8Bytes = expectedByteCount <= StackallocByteThreshold ?
            stackalloc byte[StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount));

        try
        {
            var actualByteCount = Encoding.UTF8.GetBytes(value, utf8Bytes);
            utf8Bytes = utf8Bytes[..actualByteCount];
            stream.Write(utf8Bytes);
        }
        finally
        {
            if (array is not null)
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
    }
}
