using MemoryPack;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Runtime.Serialization.JsonConstants;

namespace System.Runtime.Serialization;

/// <summary>
/// 仅用于 CoreCLR 的高效二进制序列化
/// </summary>
public partial interface IBinarySerialize
{
    [return: NotNullIfNotNull(nameof(value))]
    byte[]? Serialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(T? value)
    {
        var r = SerializeCore(value);
        return r;
    }

    T? Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(byte[]? value, T? defaultValue = default)
    {
        var r = DeserializeCore(value, defaultValue);
        return r;
    }
}

partial interface IBinarySerialize
{
    [return: NotNullIfNotNull(nameof(value))]
    static byte[]? SerializeCore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(T? value)
    {
        if (value is null)
        {
            return null;
        }

        var ttype = typeof(T);

        if (ttype == typeof(byte[]))
        {
            return Unsafe.As<T, byte[]>(ref value);
        }
        else if (ttype == typeof(string))
        {
            byte[]? buffer = null;
            try
            {
                var valueString = Unsafe.As<T, string>(ref value);
                if (valueString.Length == 0)
                {
                    return [];
                }
                else
                {
                    var len = Encoding.UTF8.GetMaxByteCount(valueString.Length);
                    Span<byte> bytes = len <= StackallocByteThreshold ?
                        stackalloc byte[StackallocByteThreshold] :
                        (buffer = ArrayPool<byte>.Shared.Rent(len)).AsSpan(0, len);
                    var bytesWritten = Encoding.UTF8.GetBytes(valueString, bytes);
                    return bytes[..bytesWritten].ToArray();
                }
            }
            finally
            {
                if (buffer != null)
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }
        else if (value is DateTime dt)
        {
            var dtBin = dt.ToBinary();
#if NET8_0_OR_GREATER
            Span<byte> utf8Destination = stackalloc byte[MaximumFormatDateTimeLengthU8];
            if (dtBin.TryFormat(utf8Destination, out var bytesWritten, default, CultureInfo.InvariantCulture))
            {
                return utf8Destination[..bytesWritten].ToArray();
            }
            else
#endif
            {
                return Encoding.UTF8.GetBytes(dtBin.ToString(CultureInfo.InvariantCulture));
            }
        }
        else if (value is DateTimeOffset dto)
        {
#if NET8_0_OR_GREATER
            Span<byte> utf8Destination = stackalloc byte[MaximumFormatDateTimeOffsetLengthU8];
            if (dto.TryFormat(utf8Destination, out var bytesWritten, "O", CultureInfo.InvariantCulture))
            {
                return utf8Destination[..bytesWritten].ToArray();
            }
            else
#endif
            {
                return Encoding.UTF8.GetBytes(dto.ToString("O"));
            }
        }
        else
        {
            return MemoryPackSerializer.Serialize(value);
        }
    }

    static T? DeserializeCore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(byte[]? value, T? defaultValue = default)
    {
        if (value == null)
        {
            return default;
        }

        var ttype = typeof(T);

        if (ttype == typeof(byte[]))
        {
            return Unsafe.As<byte[], T>(ref value);
        }
        else if (value.Length == 0)
        {
            return default;
        }
        else if (ttype == typeof(string))
        {
            if (value.Length == 0)
            {
                var valueString = string.Empty;
                return Unsafe.As<string, T>(ref valueString);
            }
            else
            {
                char[]? buffer = null;
                try
                {
                    var len = Encoding.UTF8.GetMaxCharCount(value.Length);
                    Span<char> chars = len <= StackallocByteThreshold ?
                        stackalloc char[StackallocByteThreshold] :
                        (buffer = ArrayPool<char>.Shared.Rent(len)).AsSpan(0, len);
                    var charsWritten = Encoding.UTF8.GetChars(value, chars);
                    var valueString = new string(chars[..charsWritten]);
                    return Unsafe.As<string, T>(ref valueString);
                }
                finally
                {
                    if (buffer != null)
                    {
                        ArrayPool<char>.Shared.Return(buffer);
                    }
                }
            }
        }
        else if (ttype == typeof(DateTime))
        {
            // long for the .NET 9+ format
            if (long.TryParse(value, CultureInfo.InvariantCulture, out var longValue))
            {
                var vdt = DateTime.FromBinary(longValue);
                return Unsafe.As<DateTime, T>(ref vdt);
            }
            // DateTime string for the .NET 8 format
            if (value.Length <= MaximumFormatDateTimeLengthU8)
            {
                var len = Encoding.UTF8.GetMaxCharCount(value.Length);
                Span<char> chars = stackalloc char[StackallocByteThreshold];
                if (Encoding.UTF8.TryGetChars(value, chars, out var actualLen))
                {
                    chars = chars[..actualLen];
                    if (DateTime.TryParse(chars, CultureInfo.InvariantCulture, out var datetimeValue))
                    {
                        return Unsafe.As<DateTime, T>(ref datetimeValue);
                    }
                }
            }
        }
        else if (ttype == typeof(DateTimeOffset))
        {
            if (value.Length <= MaximumFormatDateTimeOffsetLengthU8)
            {
                var len = Encoding.UTF8.GetMaxCharCount(value.Length);
                Span<char> chars = stackalloc char[StackallocByteThreshold];
                if (Encoding.UTF8.TryGetChars(value, chars, out var actualLen))
                {
                    chars = chars[..actualLen];
                    if (DateTimeOffset.TryParse(chars, CultureInfo.InvariantCulture, out var dateTimeOffset))
                    {
                        return Unsafe.As<DateTimeOffset, T>(ref dateTimeOffset);
                    }
                }
            }
        }
        else
        {
            try
            {
                return MemoryPackSerializer.Deserialize<T>(value);
            }
            catch
            {
                // bad get, fall back to default
            }
        }
        return defaultValue;
    }
}

file static class JsonConstants
{
    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v9.0.8/src/libraries/System.Text.Json/Common/JsonConstants.cs#L12
    /// </summary>
    internal const int StackallocByteThreshold = 256;

    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v10.0.0-rc.1.25451.107/src/libraries/System.Text.Json/src/System/Text/Json/JsonConstants.cs#L92
    /// </summary>
    internal const int MaximumFormatDateTimeLength = 27;    // StandardFormat 'O', e.g. 2017-06-12T05:30:45.7680000
    internal const int MaximumFormatDateTimeOffsetLength = 33;  // StandardFormat 'O', e.g. 2017-06-12T05:30:45.7680000-07:00

    internal const int MaximumFormatDateTimeLengthU8 = 84; // Encoding.UTF8.GetMaxByteCount(27)
    internal const int MaximumFormatDateTimeOffsetLengthU8 = 102; // Encoding.UTF8.GetMaxByteCount(33)
}