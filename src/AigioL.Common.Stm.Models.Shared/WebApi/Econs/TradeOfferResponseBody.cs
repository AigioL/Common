namespace AigioL.Common.Stm.Models.WebApi.Econs;

public sealed partial record class TradeOfferResponseBody
{
    public TradeOfferResponseBodyOffer Offer { get; set; } = new();
}
