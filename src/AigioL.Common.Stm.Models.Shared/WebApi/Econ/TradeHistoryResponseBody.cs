using AigioL.Common.Stm.Models.Converters;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Econ;

/// <summary>
/// Steam 市场交易历史响应正文内容模型
/// </summary>
public partial record class TradeHistoryResponseBody
{
    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool More { get; set; }

    public TradeHistoryResponseBodyTrade[] Trades { get; set; } = [];

    public TradeHistoryResponseBodyDescription[] Descriptions { get; set; } = [];
}
