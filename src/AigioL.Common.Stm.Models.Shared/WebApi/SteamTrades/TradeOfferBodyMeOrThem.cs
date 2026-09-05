using AigioL.Common.Stm.Models.Converters;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.SteamTrades;

public sealed partial record class TradeOfferBodyMeOrThem
{
    public TradeOfferBodyAsset[] Assets { get; set; } = [];

    ///// <summary>
    ///// 此字段没用，保持空数组，类型未知
    ///// </summary>
    //public JsonArray Currency { get; set; } = [];

    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool Ready { get; set; }
}
