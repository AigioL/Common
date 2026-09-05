using AigioL.Common.Stm.Models.WebApi.SteamTrades.Abstractions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.Stm.Models.WebApi.SteamTrades;

public static partial class TradeOfferIdResponseExtensions
{
    /// <summary>
    /// 从响应中读取 TradeOfferId 字段
    /// </summary>
    public static bool TryGetTradeOfferId(
        [NotNullWhen(true)] this ITradeOfferIdResponse? response,
        out ulong tradeOfferId)
    {
        if (response != null && ulong.TryParse(response.TradeOfferId, out tradeOfferId))
        {
            return true;
        }
        tradeOfferId = default;
        return false;
    }
}