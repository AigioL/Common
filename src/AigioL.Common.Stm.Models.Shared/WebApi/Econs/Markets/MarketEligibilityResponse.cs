namespace AigioL.Common.Stm.Models.WebApi.Econs.Markets;

public sealed partial class MarketEligibilityResponse
{
    public MarketEligibilityResponseBody Response { get; set; } = new();

    /// <summary>
    /// <see cref="MarketEligibilityResponse"/> 的 JSON 示例值，成功时
    /// </summary>
    public static ReadOnlySpan<byte> OkExampleValue01 =>
"""
{
  "response": {
    "allowed": true,
    "reason": 0,
    "allowed_at_time": 0,
    "steamguard_required_days": 15,
    "new_device_cooldown_days": 0
  }
}
"""u8;

    /// <summary>
    /// <see cref="MarketEligibilityResponse"/> 的 JSON 示例值，SteamId64 错误值时或返回 400 状态码
    /// </summary>
    public static ReadOnlySpan<byte> OkExampleValue02 =>
"""
{
  "response": {
    "allowed": true,
    "reason": 0
  }
}
"""u8;

    /// <summary>
    /// <see cref="MarketEligibilityResponse"/> 的 JSON 示例值，从 Cookie 值中获取，成功时
    /// </summary>
    public static ReadOnlySpan<byte> OkByCookieExampleValue01 =>
"""
{
  "allowed": 1,
  "allowed_at_time": 0,
  "steamguard_required_days": 15,
  "new_device_cooldown_days": 0,
  "time_checked": 1768380521
}
"""u8;
}