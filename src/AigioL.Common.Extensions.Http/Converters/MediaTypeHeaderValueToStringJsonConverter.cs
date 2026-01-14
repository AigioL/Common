using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.Extensions.Http.Converters;

public sealed partial class MediaTypeHeaderValueToStringJsonConverter : JsonConverter<MediaTypeHeaderValue?>
{
    public sealed override MediaTypeHeaderValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.String:
                {
                    var str = reader.GetString();
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        return null;
                    }
                    return MediaTypeHeaderValue.Parse(str);
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(reader.TokenType), reader.TokenType, null);
        }
    }

    public sealed override void Write(Utf8JsonWriter writer, MediaTypeHeaderValue? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var str = value.ToString();
        writer.WriteStringValue(str);
    }
}
