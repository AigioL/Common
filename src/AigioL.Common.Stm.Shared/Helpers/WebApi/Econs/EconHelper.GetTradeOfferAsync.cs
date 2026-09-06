using AigioL.Common.Extensions.Http.Models;
using AigioL.Common.Models;
using AigioL.Common.Stm.Constants;
using AigioL.Common.Stm.Models.WebApi.Econs;
using Microsoft.Extensions.Logging;

namespace AigioL.Common.Stm.Helpers.WebApi.Econs;

partial class EconHelper
{
    /// <summary>
    /// 获取特定交易报价
    /// <para>https://steamapi.xpaw.me/#IEconService/GetTradeOffer</para>
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="httpClient"></param>
    /// <param name="accessToken">Access key</param>
    /// <param name="tradeOfferId"></param>
    /// <param name="getDescriptions">如果设置此选项，返回的交易报价中包含的物品显示数据也将一并返回。若无法获取一个或多个描述信息，则请求将失败</param>
    /// <param name="language">加载项目显示数据时使用的语言</param>
    /// <param name="responseMessageRecord"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<ApiRsp<TradeOfferResponseBodyOffer?>> GetTradeOfferAsync(
        ILogger logger,
        HttpClient httpClient,
        string accessToken,
        ulong tradeOfferId,
        bool getDescriptions = false,
        string? language = SteamApiUrls.DefaultLanguage,
        HttpResponseMessageRecord? responseMessageRecord = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
