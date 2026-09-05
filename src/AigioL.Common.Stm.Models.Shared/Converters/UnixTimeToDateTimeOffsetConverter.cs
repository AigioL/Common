using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.Converters;

/// <summary>
/// Unix 时间戳（秒）字符串/数字 与 本地时间 <see cref="DateTimeOffset"/> 或 <see cref="DateTime"/> 互转转换器基类
/// </summary>
/// <typeparam name="TDateTime"></typeparam>
public abstract partial class UnixTimeToDateTimeOffsetConverter<TDateTime> : JsonConverter<TDateTime>
   where TDateTime : notnull
{
    /// <summary>
    /// 由子类实现的 Unix 时间戳（秒）转换为 <typeparamref name="TDateTime"/> 的方法
    /// </summary>
    protected abstract TDateTime FromUnixTime(long unixTimestamp);

    /// <inheritdoc/>
    public override TDateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                {
                    var str = reader.GetString();
                    if (long.TryParse(str, out var unixTimestamp))
                    {
                        return FromUnixTime(unixTimestamp);
                    }
                }
                break;
            case JsonTokenType.Number:
                {
                    if (reader.TryGetInt64(out var unixTimestamp))
                    {
                        return FromUnixTime(unixTimestamp);
                    }
                }
                break;
        }
        return default;
    }
}

/// <summary>
/// Unix 时间戳（秒）字符串/数字 与 本地时间 <see cref="DateTimeOffset"/> 互转转换器，写入使用 <see cref="JsonTokenType.String"/>
/// </summary>
public partial class UnixTimeSecondsStringToDateTimeOffsetConverter : UnixTimeToDateTimeOffsetConverter<DateTimeOffset>
{
    /// <inheritdoc/>
    protected override DateTimeOffset FromUnixTime(long unixTimestamp)
    {
        if (unixTimestamp == default)
        {
            return default;
        }

        var dt = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
        if (ToLocalTime)
            dt = dt.ToLocalTime();
        return dt;
    }

    /// <summary>
    /// 写入是否使用字符串，默认使用字符串
    /// </summary>
    protected virtual bool UseStringWrite => true;

    /// <summary>
    /// 读取时是否转换为本地时间，默认转换为本地时间
    /// </summary>
    protected virtual bool ToLocalTime => true;

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        var unixTimestamp = value == default ? 0L : value.ToUnixTimeSeconds();
        if (UseStringWrite)
        {
            Span<char> chars = stackalloc char[19]; // int64 最大长度 19
            if (unixTimestamp.TryFormat(chars, out var written))
            {
                var str = chars[..written];
                writer.WriteStringValue(str);
            }
            else
            {
                // 不可能进入此分支
                var str = unixTimestamp.ToString();
                writer.WriteStringValue(str);
            }
        }
        else
        {
            writer.WriteNumberValue(unixTimestamp);
        }
    }
}

/// <summary>
/// Unix 时间戳（秒）字符串/数字 与 本地时间 <see cref="DateTimeOffset"/> 互转转换器，写入使用 <see cref="JsonTokenType.Number"/>
/// </summary>
public sealed partial class UnixTimeSecondsToDateTimeOffsetConverter : UnixTimeSecondsStringToDateTimeOffsetConverter
{
    /// <inheritdoc/>
    protected sealed override bool UseStringWrite => false;
}