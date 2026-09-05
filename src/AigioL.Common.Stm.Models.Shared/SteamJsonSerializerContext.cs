using System.Text.Json;
using System.Text.Json.Serialization;
using AigioL.Common.Stm.Models.WebApi.Econ;

namespace AigioL.Common.Stm.Models;

//[JsonSerializable(typeof(SteamUserPlayerBansResponse))]
//[JsonSerializable(typeof(SteamUserPlayerSummariesResponse))]
//[JsonSerializable(typeof(MyInventoryResponse))]
//[JsonSerializable(typeof(AssetClassInfoResponse))]
//[JsonSerializable(typeof(AssetClassInfoResponseResultValue))]
//[JsonSerializable(typeof(ImmutableDictionary<long, AssetClassInfoResponseResultValue>))]
//[JsonSerializable(typeof(TradeOfferResponse))]
[JsonSerializable(typeof(TradeHistoryResponse))]
//[JsonSerializable(typeof(TradeOffersResponse))]
//[JsonSerializable(typeof(TradeOfferBody))]
//[JsonSerializable(typeof(TradeNewSendResponse))]
//[JsonSerializable(typeof(TradeCancelOrDeclineResponse))]
//[JsonSerializable(typeof(TradeAcceptResponse))]
//[JsonSerializable(typeof(MarketEligibilityResponse))]
//[JsonSerializable(typeof(JwtAjaxRefreshResponse))]
[JsonSourceGenerationOptions]
public sealed partial class SteamJsonSerializerContext : JsonSerializerContext
{
    static SteamJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new SteamJsonSerializerContext(o);
    }
}