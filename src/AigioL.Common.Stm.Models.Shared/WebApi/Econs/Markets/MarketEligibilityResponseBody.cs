using AigioL.Common.Stm.Models.Converters;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Econs.Markets;

public sealed partial record class MarketEligibilityResponseBody
{
    /// <summary>
    /// 是否允许
    /// </summary>
    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool Allowed { get; set; }

    /// <summary>
    /// 封禁原因
    /// </summary>
    public MarketNotAllowedReasonFlags Reason { get; set; }

    /// <summary>
    /// 解封时间
    /// </summary>
    [JsonPropertyName("allowed_at_time")]
    [JsonConverter(typeof(NullableUnixTimeSecondsToDateTimeOffsetConverter))]
    public DateTimeOffset? AllowedAtTime { get; set; }

    [JsonPropertyName("steamguard_required_days")]
    public int? SteamGuardRequiredDays { get; set; }

    [JsonPropertyName("new_device_cooldown_days")]
    public int? NewDeviceCooldownDays { get; set; }

    /// <summary>
    /// 检查时间
    /// <para>从 Cookie 值中获取有此属性，从发行商接口中获取则没有</para>
    /// </summary>
    [JsonPropertyName("time_checked")]
    [JsonConverter(typeof(NullableUnixTimeSecondsToDateTimeOffsetConverter))]
    public DateTimeOffset? TimeChecked { get; set; }
}