using AigioL.Common.Stm.Models.Converters;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.SteamTrades;

public sealed partial record class TradeAcceptResponse : IExplicitHasValue
{
    bool IExplicitHasValue.ExplicitHasValue()
    {
        return StrError == null;
    }
}

partial record class TradeAcceptResponse // 当出现错误时，返回仅有此字段
{
    public string? StrError { get; set; }
}

partial record class TradeAcceptResponse // 当成功时，返回的字段
{
    [JsonPropertyName("tradeid")]
    public string? TradeId { get; set; }

    /// <summary>
    /// 是否需要移动端 App 认证
    /// </summary>
    [JsonPropertyName("needs_mobile_confirmation")]
    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool NeedsMobileConfirmation { get; set; }

    /// <summary>
    /// 是否需要邮箱确认
    /// </summary>
    [JsonPropertyName("needs_email_confirmation")]
    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool NeedsEmailConfirmation { get; set; }

    [JsonPropertyName("email_domain")]
    public string? EmailDomain { get; set; }

    /// <summary>
    /// <see cref="TradeAcceptResponse"/> 的 JSON 示例值，这里需要手机令牌验证，验证后才能有 TradeId
    /// </summary>
    public static ReadOnlySpan<byte> OkExampleValue01 =>
"""
{
  "tradeid": null,
  "needs_mobile_confirmation": true,
  "needs_email_confirmation": true,
  "email_domain": "abcdefg.com"
}
"""u8;

    /// <summary>
    /// <see cref="TradeAcceptResponse"/> 的 JSON 示例值，例如对方发起交易，提供了物品，这时接受后直接返回 TradeId
    /// </summary>
    public static ReadOnlySpan<byte> OkExampleValue02 =>
"""
{
  "tradeid": "812345678912345678"
}
"""u8;
}