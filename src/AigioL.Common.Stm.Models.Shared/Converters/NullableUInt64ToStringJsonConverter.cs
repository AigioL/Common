using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.Converters;

/// <summary>
/// 将可 <see langword="null"/> 的 <see langword="ulong"/> 序列化为字符串的 <see cref="JsonConverter"/>，读取时支持字符串和数字两种格式
/// </summary>
public sealed partial class NullableUInt64ToStringJsonConverter : JsonConverter<ulong?>
{
    /// <inheritdoc/>
    public sealed override ulong? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.None:
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.String:
                {
                    var value = ulong.Parse(reader.ValueSpan);
                    return value;
                }
            default:
                {
                    var value = reader.GetUInt64();
                    return value;
                }
        }
    }

    void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
    {
        Span<char> chars = stackalloc char[20];
        if (value.TryFormat(chars, out var written))
        {
            chars = chars[..written];
            writer.WriteStringValue(chars);
        }
        else
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    /// <inheritdoc/>
    public sealed override void Write(Utf8JsonWriter writer, ulong? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }
        Write(writer, value.Value, options);
    }
}