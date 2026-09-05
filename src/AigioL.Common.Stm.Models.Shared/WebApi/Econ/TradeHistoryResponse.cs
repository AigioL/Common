namespace AigioL.Common.Stm.Models.WebApi.Econ;

/// <summary>
/// Steam 市场交易历史响应模型
/// </summary>
public sealed partial record class TradeHistoryResponse
{
    /// <inheritdoc cref="TradeHistoryResponseBody"/>
    public TradeHistoryResponseBody Response { get; set; } = new();

    /// <summary>
    /// <see cref="TradeHistoryResponse"/> 的 JSON 示例值
    /// </summary>
    public static ReadOnlySpan<byte> ExampleValue =>
"""
{
    "response": {
        "more": false,
        "trades": [
            {
                "tradeid": "123456789012345678",
                "steamid_other": "12345678901234567",
                "time_init": 1456789700,
                "status": 3,
                "assets_received": [
                    {
                        "appid": 753,
                        "contextid": "6",
                        "assetid": "1234567890",
                        "amount": "1",
                        "classid": "1234567890",
                        "instanceid": "123456789",
                        "new_assetid": "1234567890",
                        "new_contextid": "3"
                    }
                ]
            }
        ],
        "descriptions": [
            {
                "appid": 753,
                "classid": "1234567890",
                "instanceid": "123456789",
                "currency": false,
                "background_color": "",
                "icon_url": "uQqGEgxNyzAx-base64image",
                "icon_url_large": "VZG6xneuQqGEgxNyzAx-base64image",
                "descriptions": [
                    {
                        "type": "text",
                        "value": "The gray brick wall"
                    },
                    {
                        "type": "text",
                        "value": "This item can no longer be bought or sold on the Community Market."
                    }
                ],
                "tradable": true,
                "actions": [
                    {
                        "link": "https://shared.steamstatic.com/community_assets/images/items/403570/6aef8b29219ad3989c4294e3cbc68892a73a28fe.jpg",
                        "name": "View Full Size"
                    }
                ],
                "owner_actions": [
                    {
                        "link": "javascript:GetGooValue( '%contextid%', '%assetid%', 123570, 11, 0 )",
                        "name": "#TradingCards_GrindIntoGoo"
                    }
                ],
                "name": "Brick Wall",
                "type": "Stigmat Profile Background",
                "market_name": "Brick Wall",
                "market_hash_name": "403570-Brick Wall",
                "market_fee_app": 123570,
                "commodity": true,
                "market_tradable_restriction": 7,
                "market_marketable_restriction": 7,
                "marketable": false,
                "sealed": false
            }
        ]
    }
}
"""u8;
}