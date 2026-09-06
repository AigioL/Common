using AigioL.Common.Extensions.Http.Models;
using AigioL.Common.Models;
using Microsoft.Extensions.Logging;

namespace AigioL.Common.Stm.Helpers.WebApi.Communities;

partial class DevHelper
{
    /// <summary>
    /// 获取 Steam Web API 密钥
    /// <para>https://steamcommunity.com/dev/apikey</para>
    /// </summary>
    public static Task<ApiRsp<string?>> GetSteamApiKeyAsync(
        ILogger logger,
        IHttpClientFactory httpClientFactory,
        HttpClientSession session,
        HttpResponseMessageRecord? responseMessageRecord = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
