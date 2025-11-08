using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Buffers;

public static partial class BufferWriterExtensions
{
    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v9.0.8/src/libraries/System.Text.Json/Common/JsonConstants.cs#L12
    /// </summary>
    public const int StackallocByteThreshold = 256;

    public static void Write(this IBufferWriter<byte> writer, bool value, bool isLower = false)
    {
        if (value)
        {
            writer.Write(isLower ? "true"u8 : "True"u8);
        }
        else
        {
            writer.Write(isLower ? "false"u8 : "False"u8);
        }
    }

    public static void WriteByteNumber(this IBufferWriter<byte> writer, byte value)
    {
        Span<byte> buffer = stackalloc byte[3];
        bool result = value.TryFormat(buffer, out int bytesWritten, default, CultureInfo.InvariantCulture);
        if (result)
        {
            buffer = buffer[..bytesWritten];
            writer.Write(buffer);
        }
        else
        {
            writer.Write(Encoding.UTF8.GetBytes(value.ToString(CultureInfo.InvariantCulture)));
        }
    }

    public static void WriteByteNumber(this IBufferWriter<byte> writer, sbyte value)
    {
        Span<byte> buffer = stackalloc byte[4];
        bool result = value.TryFormat(buffer, out int bytesWritten, default, CultureInfo.InvariantCulture);
        if (result)
        {
            buffer = buffer[..bytesWritten];
            writer.Write(buffer);
        }
        else
        {
            writer.Write(Encoding.UTF8.GetBytes(value.ToString(CultureInfo.InvariantCulture)));
        }
    }

    public static void Write(this IBufferWriter<byte> writer, short value)
    {
        Span<byte> buffer = stackalloc byte[6];
        bool result = value.TryFormat(buffer, out int bytesWritten, default, CultureInfo.InvariantCulture);
        if (result)
        {
            buffer = buffer[..bytesWritten];
            writer.Write(buffer);
        }
        else
        {
            writer.Write(Encoding.UTF8.GetBytes(value.ToString(CultureInfo.InvariantCulture)));
        }
    }

    public static void Write(this IBufferWriter<byte> writer, ushort value)
    {
        Span<byte> buffer = stackalloc byte[5];
        bool result = value.TryFormat(buffer, out int bytesWritten, default, CultureInfo.InvariantCulture);
        if (result)
        {
            buffer = buffer[..bytesWritten];
            writer.Write(buffer);
        }
        else
        {
            writer.Write(Encoding.UTF8.GetBytes(value.ToString(CultureInfo.InvariantCulture)));
        }
    }

    public static void Write(this IBufferWriter<byte> writer, int value)
    {
        Span<byte> buffer = stackalloc byte[11];
        bool result = value.TryFormat(buffer, out int bytesWritten, default, CultureInfo.InvariantCulture);
        if (result)
        {
            buffer = buffer[..bytesWritten];
            writer.Write(buffer);
        }
        else
        {
            writer.Write(Encoding.UTF8.GetBytes(value.ToString(CultureInfo.InvariantCulture)));
        }
    }

    public static void Write(this IBufferWriter<byte> writer, uint value)
    {
        Span<byte> buffer = stackalloc byte[10];
        bool result = value.TryFormat(buffer, out int bytesWritten, default, CultureInfo.InvariantCulture);
        if (result)
        {
            buffer = buffer[..bytesWritten];
            writer.Write(buffer);
        }
        else
        {
            writer.Write(Encoding.UTF8.GetBytes(value.ToString(CultureInfo.InvariantCulture)));
        }
    }

    public static void Write(this IBufferWriter<byte> writer, long value)
    {
        Span<byte> buffer = stackalloc byte[20];
        bool result = value.TryFormat(buffer, out int bytesWritten, default, CultureInfo.InvariantCulture);
        if (result)
        {
            buffer = buffer[..bytesWritten];
            writer.Write(buffer);
        }
        else
        {
            writer.Write(Encoding.UTF8.GetBytes(value.ToString(CultureInfo.InvariantCulture)));
        }
    }

    public static void Write(this IBufferWriter<byte> writer, ulong value)
    {
        Span<byte> buffer = stackalloc byte[20];
        bool result = value.TryFormat(buffer, out int bytesWritten, default, CultureInfo.InvariantCulture);
        if (result)
        {
            buffer = buffer[..bytesWritten];
            writer.Write(buffer);
        }
        else
        {
            writer.Write(Encoding.UTF8.GetBytes(value.ToString(CultureInfo.InvariantCulture)));
        }
    }

    /// <summary>
    /// 将字符串按 HTML 编码后写入缓冲区
    /// </summary>
    public static void WriteHtmlEncodedText(this IBufferWriter<byte> writer, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        writer.WriteHtmlEncodedText(value.AsSpan());
    }

    /// <summary>
    /// 将字符 Span 按 HTML 编码后写入缓冲区
    /// </summary>
    public static void WriteHtmlEncodedText(this IBufferWriter<byte> writer, ReadOnlySpan<char> value)
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
            WriteHtmlEncodedText(writer, utf8Bytes);
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
    /// 将 utf8 字节按 HTML 编码后写入缓冲区
    /// </summary>
    public static void WriteHtmlEncodedText(this IBufferWriter<byte> writer, ReadOnlySpan<byte> value)
    {
        if (value.IsEmpty)
        {
            return;
        }

        int expectedByteCount = (int)MathF.Floor(value.Length * 1.3f);

        byte[]? array = null;
        Span<byte> utf8Bytes = expectedByteCount <= StackallocByteThreshold ?
            stackalloc byte[StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount));

        try
        {
            ReadOnlySpan<byte> remaining = value;

            // 循环处理，直到所有数据都被编码
            while (!remaining.IsEmpty)
            {
                var status = HtmlEncoder.Default.EncodeUtf8(remaining, utf8Bytes, out int bytesConsumed, out int bytesWritten);

                switch (status)
                {
                    // 编码完成，写入结果并处理剩余数据
                    case OperationStatus.Done:
                        utf8Bytes = utf8Bytes[..bytesWritten];
                        writer.Write(utf8Bytes);
                        return;
                    // 目标缓冲区太小，写入已编码的部分，然后扩大缓冲区
                    case OperationStatus.DestinationTooSmall:
                        if (bytesWritten > 0)
                        {
                            writer.Write(utf8Bytes[..bytesWritten]);
                            remaining = remaining[bytesConsumed..];
                            // 当前缓冲区大小可用，继续使用
                        }
                        else
                        {
                            // 未写入内容时扩大缓冲区
                            if (array != null)
                            {
                                ArrayPool<byte>.Shared.Return(array);
                            }

                            expectedByteCount *= 2;
                            array = ArrayPool<byte>.Shared.Rent(expectedByteCount);
                            utf8Bytes = array;
                        }
                        continue;
                    // 输入数据不完整（多字节字符被截断）
                    // 写入已处理的部分，剩余不完整的数据需要特殊处理
                    case OperationStatus.NeedMoreData:
                    // 遇到无效的 UTF-8 数据
                    case OperationStatus.InvalidData:
                    // 未知状态，退出循环
                    default:
                        throw new ArgumentOutOfRangeException(nameof(status), status, null);
                }
            }
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
    /// 将字符串按 URL 编码后写入缓冲区
    /// </summary>
    public static void WriteUrlEncodedText(this IBufferWriter<byte> writer, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        writer.WriteUrlEncodedText(value.AsSpan());
    }

    /// <summary>
    /// 将字符 Span 按 URL 编码后写入缓冲区
    /// </summary>
    public static void WriteUrlEncodedText(this IBufferWriter<byte> writer, ReadOnlySpan<char> value)
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
            WriteUrlEncodedText(writer, utf8Bytes);
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
    /// 将 utf8 字节按 URL 编码后写入缓冲区
    /// </summary>
    public static void WriteUrlEncodedText(this IBufferWriter<byte> writer, ReadOnlySpan<byte> value)
    {
        if (value.IsEmpty)
        {
            return;
        }

        int expectedByteCount = (int)MathF.Floor(value.Length * 1.3f);

        byte[]? array = null;
        Span<byte> utf8Bytes = expectedByteCount <= StackallocByteThreshold ?
            stackalloc byte[StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount));

        try
        {
            ReadOnlySpan<byte> remaining = value;

            // 循环处理，直到所有数据都被编码
            while (!remaining.IsEmpty)
            {
                var status = UrlEncoder.Default.EncodeUtf8(remaining, utf8Bytes, out int bytesConsumed, out int bytesWritten);

                switch (status)
                {
                    // 编码完成，写入结果并处理剩余数据
                    case OperationStatus.Done:
                        utf8Bytes = utf8Bytes[..bytesWritten];
                        writer.Write(utf8Bytes);
                        return;
                    // 目标缓冲区太小，写入已编码的部分，然后扩大缓冲区
                    case OperationStatus.DestinationTooSmall:
                        if (bytesWritten > 0)
                        {
                            writer.Write(utf8Bytes[..bytesWritten]);
                            remaining = remaining[bytesConsumed..];
                            // 当前缓冲区大小可用，继续使用
                        }
                        else
                        {
                            // 未写入内容时扩大缓冲区
                            if (array != null)
                            {
                                ArrayPool<byte>.Shared.Return(array);
                            }

                            expectedByteCount *= 2;
                            array = ArrayPool<byte>.Shared.Rent(expectedByteCount);
                            utf8Bytes = array;
                        }
                        continue;
                    // 输入数据不完整（多字节字符被截断）
                    // 写入已处理的部分，剩余不完整的数据需要特殊处理
                    case OperationStatus.NeedMoreData:
                    // 遇到无效的 UTF-8 数据
                    case OperationStatus.InvalidData:
                    // 未知状态，退出循环
                    default:
                        throw new ArgumentOutOfRangeException(nameof(status), status, null);
                }
            }
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
    /// 将字符串按行拆分，去除每行首尾空白字符后写入缓冲区
    /// </summary>
    public static void WriteRemoveNewLineTrim(this IBufferWriter<byte> writer, ReadOnlySpan<byte> value) => writer.WriteRemoveNewLineTrimCore(value, false);

    /// <summary>
    /// 将字符串按行拆分，去除每行首尾空白字符后写入缓冲区，保留换行符
    /// </summary>
    public static void WriteAllLineTrim(this IBufferWriter<byte> writer, ReadOnlySpan<byte> value) => writer.WriteRemoveNewLineTrimCore(value, true);

    static void WriteRemoveNewLineTrimCore(this IBufferWriter<byte> writer, ReadOnlySpan<byte> value, bool keepNewLine = false)
    {
        Span<byte> newLine = [13, 10];
        var split = value.Split(newLine);

        int i = 0;
        while (split.MoveNext())
        {
            if (keepNewLine && i != 0)
            {
                // 一些 js 格式需要保留换行符
                writer.Write(newLine);
            }

            var it = value[split.Current].Trim(" "u8).Trim("\t"u8);
            writer.Write(it);

            i++;
        }
    }

    /// <summary>
    /// 获取字符串通过 JSON 编码后的 UTF8 字节 Span 切片
    /// </summary>
    public static ReadOnlySpan<byte> GetJsonEncodedUtf8Bytes(this string? value, JavaScriptEncoder? encoder = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            return default;
        }
        return JsonEncodedText.Encode(value.AsSpan(), encoder).EncodedUtf8Bytes;
    }

    /// <summary>
    /// 将字符串通过 JSON 编码后写入缓冲区
    /// </summary>
    public static void WriteJsonEncodedText(this IBufferWriter<byte> writer, string? value, JavaScriptEncoder? encoder = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        writer.Write(JsonEncodedText.Encode(value.AsSpan(), encoder).EncodedUtf8Bytes);
    }
}
