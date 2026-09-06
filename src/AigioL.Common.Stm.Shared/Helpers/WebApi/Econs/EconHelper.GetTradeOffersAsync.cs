using AigioL.Common.Extensions.Http.Models;
using AigioL.Common.Models;
using AigioL.Common.Stm.Constants;
using AigioL.Common.Stm.Models.WebApi.Econs;
using Microsoft.Extensions.Logging;

namespace AigioL.Common.Stm.Helpers.WebApi.Econs;

partial class EconHelper
{
    /// <summary>
    /// 获取已发送或已接收的交易报价邀约列表
    /// <para>https://steamapi.xpaw.me/#IEconService/GetTradeOffers</para>
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="httpClient"></param>
    /// <param name="accessToken">Access key</param>
    /// <param name="getSentOffers">请求发送的报价清单</param>
    /// <param name="getReceivedOffers">请求已接收报价的列表</param>
    /// <param name="getDescriptions">如果设置此选项，系统将同时返回交易要约中包含的物品的展示数据。若无法获取一项或多项描述信息，则请求将失败</param>
    /// <param name="language">加载项目显示数据时使用的语言</param>
    /// <param name="activeOnly">表示我们应仅返回仍处于有效状态的优惠，或自历史时间截止点后状态发生变更的优惠</param>
    /// <param name="historicalOnly">表示我们应仅返回非活跃的报价</param>
    /// <param name="timeHistoricalCutoff">当设置 activeOnly 时，将同时返回自该时间点起更新的报价。当设置 historicalOnly 时，仅包含自该时间点起更新的报价</param>
    /// <param name="cursor">游标，又称起始索引</param>
    /// <param name="responseMessageRecord"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<ApiRsp<TradeOffersResponseBody?>> GetTradeOffersAsync(
        ILogger logger,
        HttpClient httpClient,
        string accessToken,
        bool getSentOffers,
        bool getReceivedOffers,
        bool getDescriptions = false,
        string? language = SteamApiUrls.DefaultLanguage,
        bool activeOnly = false,
        bool historicalOnly = false,
        long? timeHistoricalCutoff = null,
        uint? cursor = null,
        HttpResponseMessageRecord? responseMessageRecord = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
