namespace AigioL.Common.Stm.Models.WebApi.Econs;

public sealed partial record class TradeOffersResponse
{
    public TradeOffersResponseBody Response { get; set; } = new();

    /// <summary>
    /// <see cref="TradeOffersResponse"/> 的 JSON 示例值
    /// </summary>
    public static ReadOnlySpan<byte> ExampleValue =>
"""
{
    "response": {
        "trade_offers_sent": [
            {
                "tradeofferid": "1234567890",
                "accountid_other": 1234567,
                "message": "",
                "expiration_time": 1362958116,
                "trade_offer_state": 2,
                "items_to_receive": [
                    {
                        "appid": 730,
                        "contextid": "2",
                        "assetid": "12345678901",
                        "classid": "1234567890",
                        "instanceid": "1234567890",
                        "amount": "1",
                        "missing": false,
                        "est_usd": "13"
                    }
                ],
                "is_our_offer": true,
                "time_created": 1362958116,
                "time_updated": 1362958116,
                "from_real_time_trade": false,
                "escrow_end_date": 0,
                "confirmation_method": 0,
                "eresult": 1,
                "delay_settlement": true,
                "settlement_date": 0
            },
            {
                "tradeofferid": "1234567890",
                "accountid_other": 1234567,
                "message": "",
                "expiration_time": 1362958116,
                "trade_offer_state": 6,
                "items_to_give": [
                    {
                        "appid": 730,
                        "contextid": "2",
                        "assetid": "12345678901",
                        "classid": "1234567890",
                        "instanceid": "1234567890",
                        "amount": "1",
                        "missing": false,
                        "est_usd": "3"
                    }
                ],
                "is_our_offer": true,
                "time_created": 1362958116,
                "time_updated": 1362958116,
                "from_real_time_trade": false,
                "escrow_end_date": 0,
                "confirmation_method": 2,
                "eresult": 1,
                "delay_settlement": true,
                "settlement_date": 0
            }
        ],
        "next_cursor": 0
    }
}
"""u8;
}