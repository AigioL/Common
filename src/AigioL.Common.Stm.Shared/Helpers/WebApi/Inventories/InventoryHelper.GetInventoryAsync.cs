using AigioL.Common.Extensions.Http.Models;
using AigioL.Common.Models;
using AigioL.Common.Stm.Constants;
using AigioL.Common.Stm.Models.WebApi.Inventories;
using Microsoft.Extensions.Logging;

namespace AigioL.Common.Stm.Helpers.WebApi.Inventories;

partial class InventoryHelper
{
    /// <summary>
    /// 获取 Steam 用户的库存资产，模拟 https://steamcommunity.com/id/{0}/inventory 页面行为的分页请求
    /// </summary>
    public static Task<ApiRsp<MyInventoryResponse?>> GetInventoryAsync(
        ILogger logger,
        HttpClientSession session,
        long steamId64,
        int appId = 730,
        int contextId = 2,
        uint count = DefFirstCount,
        int? preserveBbcode = 1,
        int? rawAssetProperties = 1,
        string? language = SteamApiUrls.DefaultLanguage_Client,
        ulong? startAssetId = null,
        HttpResponseMessageRecord? responseMessageRecord = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
