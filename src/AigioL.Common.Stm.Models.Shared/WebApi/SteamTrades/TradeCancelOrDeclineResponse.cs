using AigioL.Common.Stm.Models.WebApi.SteamTrades.Abstractions;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.SteamTrades;

public sealed partial record class TradeCancelOrDeclineResponse : IExplicitHasValue, ITradeOfferIdResponse
{
    bool IExplicitHasValue.ExplicitHasValue()
    {
        return !string.IsNullOrWhiteSpace(TradeOfferId);
    }

    // <a href="javascript:CancelTradeOffer( '1234567890' );" class="whiteLink">取消交易请求</a>
}

partial record class TradeCancelOrDeclineResponse // 当出现错误时，返回仅有此字段
{
    public long? Success { get; set; }

    /// <summary>
    /// <see cref="TradeCancelOrDeclineResponse"/> 的 JSON 示例值，当返回错误时的可能值之一，示例值
    /// <para>500 Internal Server Error 取消交易请求 修改此交易请求时发生错误。请稍后再试</para>
    /// </summary>
    public static ReadOnlySpan<byte> ErrorExampleValue01 =>
"""
{"success":42}
"""u8;
}

partial record class TradeCancelOrDeclineResponse // 当成功时，返回的字段
{
    [JsonPropertyName("tradeofferid")]
    public string? TradeOfferId { get; set; }

    /// <summary>
    /// <see cref="TradeCancelOrDeclineResponse"/> 的 JSON 示例值，当返回成功时的示例值
    /// </summary>
    public static ReadOnlySpan<byte> OkExampleValue01 =>
"""
{
  "tradeofferid": "1234567890",
}
"""u8;
}