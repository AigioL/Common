using AigioL.Common.Stm.Models.WebApi.SteamTrades.Abstractions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.Stm.Models.WebApi.SteamTrades;

public static partial class TradeOfferIdWithStrErrorResponseExtensions
{
    /// <summary>
    /// 从响应中读取 TradeOfferId 字段，若读取失败则返回错误信息
    /// </summary>
    public static bool TryGetTradeOfferId<TResponse>(
        [NotNullWhen(true)] this TResponse? response,
        out ulong tradeOfferId,
        [NotNullWhen(false)] out string? error)
        where TResponse : ITradeOfferIdWithStrErrorResponse
    {
        error = null;
        if (response != null && ulong.TryParse(response.TradeOfferId, out tradeOfferId))
        {
            return true;
        }
        tradeOfferId = default;
        error = response?.StrError ?? $"从 {typeof(TResponse)} 中读取 TradeOfferId 失败且 StrError 为 null 值";
        return false;
    }
}
