namespace AigioL.Common.Stm.Models.WebApi.SteamTrades.Abstractions;

public partial interface ITradeOfferIdWithStrErrorResponse : ITradeOfferIdResponse
{
    string? StrError { get; }
}