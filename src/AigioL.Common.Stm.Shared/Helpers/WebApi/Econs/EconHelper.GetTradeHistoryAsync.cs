using AigioL.Common.Extensions.Http.Models;
using AigioL.Common.Models;
using AigioL.Common.Stm.Constants;
using AigioL.Common.Stm.Models.WebApi.Econs;
using Microsoft.Extensions.Logging;

namespace AigioL.Common.Stm.Helpers.WebApi.Econs;

partial class EconHelper
{
    /// <summary>
    /// 获取交易历史记录
    /// <para>https://steamapi.xpaw.me/#IEconService/GetTradeHistory</para>
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="httpClient"></param>
    /// <param name="accessToken">Access key</param>
    /// <param name="maxTrades">需要返回信息的交易数量</param>
    /// <param name="startAfterTime">上页结果中显示的最后一次交易时间，或若从该页面返回时显示的首次交易时间</param>
    /// <param name="startAfterTradeId">上页结果中显示的交易 Id，或若返回则为首笔交易的 Id</param>
    /// <param name="navigatingBack">用户需要返回结果的前一页，因此应返回起始时间和 Id 之前的前 maxTrades 笔交易记录</param>
    /// <param name="getDescriptions">如果设置，则返回的交易中包含的项目的显示数据也将被返回</param>
    /// <param name="language">加载项目显示数据时使用的语言</param>
    /// <param name="includeFailed"></param>
    /// <param name="includeTotal">若设置此选项，响应中将包含该账户参与的交易总数</param>
    /// <param name="responseMessageRecord"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<ApiRsp<TradeHistoryResponseBody?>> GetTradeHistoryAsync(
        ILogger logger,
        HttpClient httpClient,
        string accessToken,
        uint maxTrades,
        long? startAfterTime = null,
        ulong? startAfterTradeId = null,
        bool navigatingBack = false,
        bool getDescriptions = false,
        string? language = SteamApiUrls.DefaultLanguage,
        bool includeFailed = false,
        bool includeTotal = false,
        HttpResponseMessageRecord? responseMessageRecord = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
