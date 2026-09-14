using AigioL.Common.Extensions.Http.Models;
using AigioL.Common.Models;
using AigioL.Common.Stm.Constants;
using AigioL.Common.Stm.Models.WebApi.Inventories;
using Microsoft.Extensions.Logging;

namespace AigioL.Common.Stm.Helpers.WebApi.Inventories;

partial class InventoryHelper
{
    /// <summary>
    /// 获取 Steam 用户的库存所有资产，使用委托动态控制每次请求的数量
    /// </summary>
    public static Task<ApiRsp<MyInventoryResponse?>> GetAllInventoryAsync(
        ILogger logger,
        HttpClientSession session,
        long steamId64,
        int appId = 730,
        int contextId = 2,
        int? preserveBbcode = 1,
        int? rawAssetProperties = 1,
        string? language = SteamApiUrls.DefaultLanguage_Client,
        Func<long, uint>? getCountDelegate = null,
        Func<long, HttpResponseMessageRecord?>? getResponseMessageRecordDelegate = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
