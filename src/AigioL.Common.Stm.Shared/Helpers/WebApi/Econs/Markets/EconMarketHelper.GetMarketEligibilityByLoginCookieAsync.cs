using AigioL.Common.Extensions.Http.Models;
using AigioL.Common.Models;
using AigioL.Common.Stm.Models.WebApi.Econs.Markets;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AigioL.Common.Stm.Helpers.WebApi.Econs.Markets;

partial class EconMarketHelper
{
    /// <summary>
    /// 检查是否允许用户账户使用市场（通过用户登录 Cookies 查询）
    /// </summary>
    public static Task<ApiRsp<MarketEligibilityResponseBody?>> GetMarketEligibilityByLoginCookieAsync(
        ILogger logger,
        HttpClient httpClient,
        CookieContainer cookieContainer,
        HttpResponseMessageRecord? responseMessageRecord = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
