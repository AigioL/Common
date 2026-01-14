using Microsoft.IO;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.Extensions.Http.Converters;

public sealed partial class RecyclableMemoryStreamToStringJsonConverter : JsonConverter<RecyclableMemoryStream?>
{
    static readonly RecyclableMemoryStreamManager m = new();

    public sealed override RecyclableMemoryStream? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.String:
                {
                    var stream = m.GetStream();

                    // https://github.com/dotnet/runtime/blob/v10.0.2/src/libraries/System.Text.Json/src/System/Text/Json/Reader/Utf8JsonReader.TryGet.cs#L27

                    if (reader.ValueIsEscaped)
                    {
                        // 取消转义未公开，只能通过获取字符串转换
                        var str = reader.GetString();
                        stream.Write(str);
                    }
                    else
                    {
                        if (reader.HasValueSequence)
                        {
                            foreach (var it in reader.ValueSequence)
                            {
                                stream.Write(it.Span);
                            }
                        }
                        else
                        {
                            stream.Write(reader.ValueSpan);
                        }
                    }

                    return stream;
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(reader.TokenType), reader.TokenType, null);
        }
    }

    public sealed override void Write(Utf8JsonWriter writer, RecyclableMemoryStream? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var seq = value.GetReadOnlySequence();
        foreach (var it in seq)
        {
            writer.WriteStringValueSegment(it.Span, false);
        }
        writer.WriteStringValueSegment(ReadOnlySpan<byte>.Empty, true);
    }
}
