using AigioL.Common.Extensions.Http.Models;
using AigioL.Common.Models;
using AigioL.Common.Stm.Models.WebApi.Econs.Markets;
using Microsoft.Extensions.Logging;

namespace AigioL.Common.Stm.Helpers.WebApi.Econs.Markets;

partial class EconMarketHelper
{
    /// <summary>
    /// 检查是否允许用户账户使用市场（通过发行商 Key 查询）
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="httpClient"></param>
    /// <param name="partnerApiKey">Steam 发行商 Key</param>
    /// <param name="steamId">要查询的 Steam 64 位用户 Id</param>
    /// <param name="responseMessageRecord"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<ApiRsp<MarketEligibilityResponseBody?>> GetMarketEligibilityByPartnerApiKeyAsync(
        ILogger logger,
        HttpClient httpClient,
        string partnerApiKey,
        ulong steamId,
        HttpResponseMessageRecord? responseMessageRecord = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
