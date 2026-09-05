namespace AigioL.Common.Stm.Models.WebApi.Econs;

public sealed partial record class TradeOfferResponse
{
    public TradeOfferResponseBody Response { get; set; } = new();

    /// <summary>
    /// <see cref="TradeOfferResponse"/> 的 JSON 示例值
    /// </summary>
    public static ReadOnlySpan<byte> ExampleValue =>
"""
{
    "response": {
        "offer": {
            "tradeofferid": "1234567890",
            "accountid_other": 1234567,
            "message": "",
            "expiration_time": 1838667190,
            "trade_offer_state": 9,
            "items_to_give": [
                {
                    "appid": 730,
                    "contextid": "2",
                    "assetid": "12345678901",
                    "classid": "1234567890",
                    "instanceid": "1234567890",
                    "amount": "1",
                    "missing": false,
                    "est_usd": "35"
                }
            ],
            "is_our_offer": true,
            "time_created": 1217322690,
            "time_updated": 1217322690,
            "from_real_time_trade": false,
            "escrow_end_date": 0,
            "confirmation_method": 2,
            "eresult": 1,
            "delay_settlement": true,
            "settlement_date": 123
        }
    }
}
"""u8;
}
