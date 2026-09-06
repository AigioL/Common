using AigioL.Common.Extensions.Http.Models;
using AigioL.Common.Models;
using Microsoft.Extensions.Logging;

namespace AigioL.Common.Stm.Helpers.WebApi.Econs;

partial class EconHelper
{
    /// <summary>
    /// 获取我的交易报价访问 Url
    /// </summary>
    public static Task<ApiRsp<Uri?>> GetTradeOfferAccessUrlAsync(
        ILogger logger,
        ulong steam64Id,
        HttpClient httpClient,
        HttpResponseMessageRecord? responseMessageRecord = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
