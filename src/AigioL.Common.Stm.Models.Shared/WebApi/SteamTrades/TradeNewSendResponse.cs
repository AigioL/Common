using AigioL.Common.Stm.Models.WebApi.SteamTrades.Abstractions;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.SteamTrades;

public sealed partial record class TradeNewSendResponse : IExplicitHasValue, ITradeOfferIdWithStrErrorResponse
{
    bool IExplicitHasValue.ExplicitHasValue()
    {
        return StrError == null && !string.IsNullOrWhiteSpace(TradeOfferId);
    }
}

partial record class TradeNewSendResponse // 当出现错误时，返回仅有此字段
{
    public string? StrError { get; set; }

    /// <summary>
    /// <see cref="TradeNewSendResponse"/> 的 JSON 示例值，当返回错误时的可能值之一，示例值
    /// </summary>
    public static ReadOnlySpan<byte> ErrorExampleValue01 =>
"""
{strError: "您发送的交易请求过多，或是与 xxx 之间有过多尚未完成的交易请求。在发送更多交易请求前，请先取消一些。"}
"""u8;
}

partial record class TradeNewSendResponse // 当成功时，返回的字段
{
    [JsonPropertyName("email_domain")]
    public string? EmailDomain { get; set; }

    /// <summary>
    /// 是否需要邮箱确认
    /// </summary>
    [JsonPropertyName("needs_email_confirmation")]
    public bool NeedsEmailConfirmation { get; set; }

    /// <summary>
    /// 是否需要移动端 App 认证
    /// </summary>
    [JsonPropertyName("needs_mobile_confirmation")]
    public bool NeedsMobileConfirmation { get; set; }

    [JsonPropertyName("tradeofferid")]
    public string? TradeOfferId { get; set; }

    /// <summary>
    /// <see cref="TradeNewSendResponse"/> 的 JSON 示例值，当返回成功时的示例值
    /// </summary>
    public static ReadOnlySpan<byte> ValueOk01 =>
"""
{
  "tradeofferid": "1234567890",
  "needs_mobile_confirmation": true,
  "needs_email_confirmation": false,
  "email_domain": "abcdefg.com"
}
"""u8;
}
