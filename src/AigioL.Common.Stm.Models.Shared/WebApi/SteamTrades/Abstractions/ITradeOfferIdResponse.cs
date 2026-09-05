namespace AigioL.Common.Stm.Models.WebApi.SteamTrades.Abstractions;

public partial interface ITradeOfferIdResponse
{
    /// <summary>
    /// 交易报价 Id，值为 10 位或更长的数字，类型应使用 <see langword="ulong"/>
    /// </summary>
    string? TradeOfferId { get; }
}