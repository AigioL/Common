namespace System.Text.Json.Serialization;

/// <summary>
/// 将字符串类型直接使用为原始 JSON 内容进行序列化和反序列化，且不验证字符串是否符合 RFC 8259 标准
/// </summary>
public sealed class UnsafeRawJsonConverter : JsonConverter<string>
{
    /// <inheritdoc/>
    public sealed override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var r = doc.RootElement.GetRawText();
        return r;
    }

    /// <inheritdoc/>
    public sealed override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(value, true);
    }
}
