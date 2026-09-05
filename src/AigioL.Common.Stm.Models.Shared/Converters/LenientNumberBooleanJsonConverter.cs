using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.Converters;

/// <summary>
/// 宽容的数字布尔值转换器，支持将数字 0/1 或字符串 "true"/"false" 转换为布尔值
/// </summary>
public sealed partial class LenientNumberBooleanJsonConverter : JsonConverter<bool>
{
    /// <inheritdoc/>
    public sealed override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                {
                    // 快速路径匹配，几个固定字符串
                    if (reader.ValueTextEquals("1"u8) || reader.ValueTextEquals("true"u8) || reader.ValueTextEquals("True"u8))
                    {
                        return true;
                    }
                    else if (reader.ValueTextEquals("0"u8) || reader.ValueTextEquals("false"u8) || reader.ValueTextEquals("False"u8))
                    {
                        return false;
                    }
                    else
                    {
                        // 获取字符串值，进行不区分大小写的比较
                        var str = reader.GetString();
                        if (string.Equals("true", str, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return true;
                        }
                        else if (string.Equals("false", str, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return false;
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException(nameof(str), str, null);
                        }
                    }
                }
            case JsonTokenType.Number:
                if (reader.TryGetInt16(out var num))
                {
                    // 根据数字值返回对应的布尔值
                    return num switch
                    {
                        0 => false,
                        1 => true,
                        _ => throw new ArgumentOutOfRangeException(nameof(num), num, null),
                    };
                }
                else if (reader.TryGetDouble(out var d))
                {
                    throw new ArgumentOutOfRangeException(nameof(d), d, null);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(reader), null, null);
                }
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.Null:
            case JsonTokenType.None:
                return default;
            default:
                throw new ArgumentOutOfRangeException(nameof(reader.TokenType), reader.TokenType, null);
        }
    }

    /// <inheritdoc/>
    public sealed override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}