using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Inventories;

public sealed partial record class MyInventoryResponse
{
    public int Rwgrsn { get; set; }

    public int Success { get; set; }

    [JsonPropertyName("total_inventory_count")]
    public int TotalInventoryCount { get; set; }

    public InventoryAsset[] Assets { get; set; } = [];

    public InventoryAssetDescription[] Descriptions { get; set; } = [];

    [JsonPropertyName("asset_properties")]
    public InventoryAssetProperty[] AssetProperties { get; set; } = [];

    [JsonPropertyName("last_assetid")]
    public ulong? LastAssetId { get; set; }

    [return: NotNullIfNotNull(nameof(right))]
    public static MyInventoryResponse? operator +(MyInventoryResponse? left, MyInventoryResponse? right)
    {
        if (left == null)
        {
            return right;
        }
        else if (right == null)
        {
            return left;
        }

        var r = new MyInventoryResponse()
        {
            Rwgrsn = right.Rwgrsn,
            Success = right.Success,
            TotalInventoryCount = right.TotalInventoryCount,
            LastAssetId = right.LastAssetId,
            Assets = [.. left.Assets, .. right.Assets],
            Descriptions = [.. left.Descriptions, .. right.Descriptions],
            AssetProperties = [.. left.AssetProperties, .. right.AssetProperties],
        };
        return r;
    }

    /// <summary>
    /// <see cref="MyInventoryResponse"/> 的 JSON 示例值
    /// </summary>
    public static ReadOnlySpan<byte> ExampleValue =>
"""
{
    "assets": [
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "5489485857",
            "classid": "667924416",
            "instanceid": "667076610",
            "amount": "236"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "26106175069",
            "classid": "4127257405",
            "instanceid": "246376172",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "28463631818",
            "classid": "5414912563",
            "instanceid": "246376172",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17459165264",
            "classid": "4481569369",
            "instanceid": "3865004543",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "20156003779",
            "classid": "4605165300",
            "instanceid": "3873495864",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19170020381",
            "classid": "4681798145",
            "instanceid": "3873495864",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "8144640235",
            "classid": "157508104",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "3900657926",
            "classid": "157508104",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "3900692510",
            "classid": "157508104",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "3901868375",
            "classid": "157507746",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "20814138099",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17858705902",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "27547596901",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "20771753136",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "20761921322",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17064018080",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17624914368",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "20710276071",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17003151605",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "20569550510",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "20446616917",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17843622771",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "25888452220",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "25014199742",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "24980788391",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19224405851",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "20107144622",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "20079380555",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "18476242663",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "24935640235",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19934872443",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19835839466",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19813009851",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "24927325412",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19762244802",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19740717828",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19732710079",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "24917132811",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17470951985",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "23901764440",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19694403627",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "22961196634",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "18610722125",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "16896383392",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17135363806",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "21798582342",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19614879681",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19604963101",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "21782613050",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19601344322",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19559100423",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "21686426227",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19536388265",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17150911892",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19500486680",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "18389808924",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "16898069484",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19366544651",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19341548334",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19333465529",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19308360929",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17831049815",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17124144292",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "33231823902",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "18533600540",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "18391226336",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17356541784",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17121048819",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17330434391",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "19063717787",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17329116751",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17328525655",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "18586412548",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17327262628",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        },
        {
            "appid": 753,
            "contextid": "6",
            "assetid": "17808983937",
            "classid": "149757868",
            "instanceid": "246376127",
            "amount": "1"
        }
    ],
    "descriptions": [
        {
            "appid": 753,
            "classid": "667924416",
            "instanceid": "667076610",
            "currency": 0,
            "background_color": "",
            "icon_url": "a0b44yVZG6xneuQqGEgxNyzAxlmouy8Ipo0bUlJjRDfF9dQS1eD3AHU-t8xnioHyGdqSZpgsmMIM6FMBcAeKV6g67Fhn3ksGm3jYaprvT6LAoD77-tkGRaONtrejicS8rnFT3sd0a5Jde4IWgft4Xw",
            "icon_url_large": "a0b44yVZG6xneuQqGEgxNyzAxlmouy8Ipo0bUlJjRDfF9dQS1eD3AHU-t8xnioHyGdqSZpgsmMIM6FMBcAeKVf8_61Ex2BcBmyuPP8_uFqiboT3-8t1XEaSM5-j0gZG2q3FUhJZ0bpJde4KGPJYfFw",
            "descriptions": [
                {
                    "value": ""
                }
            ],
            "tradable": 1,
            "name": "宝石",
            "type": "Steam 宝石",
            "market_name": "宝石",
            "market_hash_name": "753-Gems",
            "market_fee_app": 753,
            "commodity": 1,
            "market_tradable_restriction": 7,
            "market_marketable_restriction": 7,
            "marketable": 0,
            "tags": [
                {
                    "category": "droprate",
                    "internal_name": "droprate_0",
                    "localized_category_name": "稀有度",
                    "localized_tag_name": "普通"
                },
                {
                    "category": "Game",
                    "internal_name": "app_753",
                    "localized_category_name": "游戏",
                    "localized_tag_name": "Steam"
                },
                {
                    "category": "Event",
                    "internal_name": "wintersale2014",
                    "localized_category_name": "活动",
                    "localized_tag_name": "2014年节庆特卖"
                },
                {
                    "category": "item_class",
                    "internal_name": "item_class_7",
                    "localized_category_name": "物品类型",
                    "localized_tag_name": "宝石"
                }
            ],
            "sealed": 0
        },
        {
            "appid": 753,
            "classid": "4127257405",
            "instanceid": "246376172",
            "currency": 0,
            "background_color": "",
            "icon_url": "IzMF03bk9WpSBq-S-ekoE33L-iLqGFHVaU25ZzQNQcXdBnY7ltYLvVIHHqLGePAREJlx6TqJAJTSjYBRkTBZjn1fMCelznlsaekP3qt5mA",
            "icon_url_large": "IzMF03bk9WpSBq-S-ekoE33L-iLqGFHVaU25ZzQNQcXdBnY7ltYLvVIHHqLGePAREJlx6TqJAJTSjYBRkTBZjn1fMCel0XV_YuBX2l3YDQ",
            "descriptions": [
                {
                    "value": ""
                },
                {
                    "type": "bbcode",
                    "value": "含有来自以下套组的三张集换式卡牌：[container_item_list][/container_item_list] 每张卡牌都有机会替换为闪亮版本。"
                }
            ],
            "tradable": 1,
            "name": "赛博朋克 2077 补充包",
            "type": "补充包",
            "market_name": "赛博朋克 2077 补充包",
            "market_hash_name": "1091500-Cyberpunk 2077 Booster Pack",
            "market_fee_app": 1091500,
            "commodity": 1,
            "market_tradable_restriction": 7,
            "market_marketable_restriction": 7,
            "marketable": 1,
            "tags": [
                {
                    "category": "droprate",
                    "internal_name": "droprate_0",
                    "localized_category_name": "稀有度",
                    "localized_tag_name": "普通"
                },
                {
                    "category": "Game",
                    "internal_name": "app_1091500",
                    "localized_category_name": "游戏",
                    "localized_tag_name": "赛博朋克 2077"
                },
                {
                    "category": "item_class",
                    "internal_name": "item_class_5",
                    "localized_category_name": "物品类型",
                    "localized_tag_name": "补充包"
                }
            ],
            "sealed": 0,
            "container_properties": {
                "contained_items": [
                    {
                        "classid": "4127253439",
                        "instanceid": "0"
                    },
                    {
                        "classid": "4127253310",
                        "instanceid": "0"
                    },
                    {
                        "classid": "4127253315",
                        "instanceid": "0"
                    },
                    {
                        "classid": "4127253923",
                        "instanceid": "0"
                    },
                    {
                        "classid": "4127253521",
                        "instanceid": "0"
                    }
                ],
                "search_tags": [
                    {
                        "appid": 753,
                        "category": "Game",
                        "internal_name": "app_1091500"
                    },
                    {
                        "appid": 753,
                        "category": "item_class",
                        "internal_name": "item_class_2"
                    }
                ]
            }
        },
        {
            "appid": 753,
            "classid": "5414912563",
            "instanceid": "246376172",
            "currency": 0,
            "background_color": "",
            "icon_url": "IzMF03bk9WpSBq-S-ekoE33L-iLqGFHVaU25ZzQNQcXdBnY7ltYLvVIHHqLGePcRFZp56TqJAJTSjYBRkTBZjn1fMCelznlsaenvJWanjw",
            "icon_url_large": "IzMF03bk9WpSBq-S-ekoE33L-iLqGFHVaU25ZzQNQcXdBnY7ltYLvVIHHqLGePcRFZp56TqJAJTSjYBRkTBZjn1fMCel0XV_YuCSPelabw",
            "descriptions": [
                {
                    "value": ""
                },
                {
                    "type": "bbcode",
                    "value": "含有来自以下套组的三张集换式卡牌：[container_item_list][/container_item_list] 每张卡牌都有机会替换为闪亮版本。"
                }
            ],
            "tradable": 1,
            "name": "Vampire Survivors - 吸血鬼幸存者 补充包",
            "type": "补充包",
            "market_name": "Vampire Survivors - 吸血鬼幸存者 补充包",
            "market_hash_name": "1794680-Vampire Survivors Booster Pack",
            "market_fee_app": 1794680,
            "commodity": 1,
            "market_tradable_restriction": 7,
            "market_marketable_restriction": 7,
            "marketable": 1,
            "tags": [
                {
                    "category": "droprate",
                    "internal_name": "droprate_0",
                    "localized_category_name": "稀有度",
                    "localized_tag_name": "普通"
                },
                {
                    "category": "Game",
                    "internal_name": "app_1794680",
                    "localized_category_name": "游戏",
                    "localized_tag_name": "Vampire Survivors - 吸血鬼幸存者"
                },
                {
                    "category": "item_class",
                    "internal_name": "item_class_5",
                    "localized_category_name": "物品类型",
                    "localized_tag_name": "补充包"
                }
            ],
            "sealed": 0,
            "container_properties": {
                "contained_items": [
                    {
                        "classid": "5414893717",
                        "instanceid": "0"
                    },
                    {
                        "classid": "5414893725",
                        "instanceid": "0"
                    },
                    {
                        "classid": "5414893729",
                        "instanceid": "0"
                    },
                    {
                        "classid": "5414893715",
                        "instanceid": "0"
                    },
                    {
                        "classid": "5414893716",
                        "instanceid": "0"
                    }
                ],
                "search_tags": [
                    {
                        "appid": 753,
                        "category": "Game",
                        "internal_name": "app_1794680"
                    },
                    {
                        "appid": 753,
                        "category": "item_class",
                        "internal_name": "item_class_2"
                    }
                ]
            }
        },
        {
            "appid": 753,
            "classid": "4481569369",
            "instanceid": "3865004543",
            "currency": 0,
            "background_color": "",
            "icon_url": "a0b44yVZG6xneuQqGEgxNyzAxlmouy8Ipo0bUlJjRDfF9dQS1eD3AHU-t8xnioHyGdqSZpgsmMIM6FMBdgSKV_89u1Fi2EgEynjZa8njFfDEp2n6qIhVRPbR4-WmiMW98XhR3pZwaZJHZYKFMgpRQg",
            "icon_url_large": "a0b44yVZG6xneuQqGEgxNyzAxlmouy8Ipo0bUlJjRDfF9dQS1eD3AHU-t8xnioHyGdqSZpgsmMIM6FMBdgSKVvs5vFc0gxsCynKJOJ3iFaeSpzKr8thVFqDQ5bf8jcSx_HYG2pEmOZJHZYIg57r5gw",
            "descriptions": [
                {
                    "value": ""
                }
            ],
            "tradable": 0,
            "name": "《反恐精英：全球攻势》个人资料",
            "type": "Counter-Strike 2 个人资料修饰器",
            "market_name": "《反恐精英：全球攻势》个人资料",
            "market_hash_name": "730-Counter-Strike: Global Offensive Profile",
            "market_fee_app": 730,
            "commodity": 1,
            "market_tradable_restriction": 7,
            "market_marketable_restriction": 7,
            "marketable": 0,
            "tags": [
                {
                    "category": "droprate",
                    "internal_name": "droprate_0",
                    "localized_category_name": "稀有度",
                    "localized_tag_name": "普通"
                },
                {
                    "category": "Game",
                    "internal_name": "app_730",
                    "localized_category_name": "游戏",
                    "localized_tag_name": "Counter-Strike 2"
                },
                {
                    "category": "item_class",
                    "internal_name": "item_class_8",
                    "localized_category_name": "物品类型",
                    "localized_tag_name": "个人资料修饰器"
                }
            ],
            "sealed": 0
        },
        {
            "appid": 753,
            "classid": "4605165300",
            "instanceid": "3873495864",
            "currency": 0,
            "background_color": "",
            "icon_url": "a0b44yVZG6xneuQqGEgxNyzAxlmouy8Ipo0bUlJjRDfF9dQS1eD3AHU-t8xnioHyGdqSZpgsmMIM6FMCcAKTVKohuAJuiRhWn32MY87pFKiVozL8qNgDFqTYsrTxjsTgqnZRjsBybNkZJsut4_2p0eam3w",
            "icon_url_large": "a0b44yVZG6xneuQqGEgxNyzAxlmouy8Ipo0bUlJjRDfF9dQS1eD3AHU-t8xnioHyGdqSZpgsmMIM6FMCcAKTVKohtwMyjx4HmnmOYsjiFqCRpDus_9wBRf2Nt7Wn38KzriBQhZd1ad0ad8ut4_0UUKMBew",
            "descriptions": [
                {
                    "value": ""
                }
            ],
            "tradable": 0,
            "name": "Hand of Fate 2 Profile",
            "type": "Hand of Fate 2 个人资料修饰器",
            "market_name": "Hand of Fate 2 Profile",
            "market_hash_name": "456670-Hand of Fate 2 Profile",
            "market_fee_app": 456670,
            "commodity": 1,
            "market_tradable_restriction": 7,
            "market_marketable_restriction": 7,
            "marketable": 0,
            "tags": [
                {
                    "category": "droprate",
                    "internal_name": "droprate_0",
                    "localized_category_name": "稀有度",
                    "localized_tag_name": "普通"
                },
                {
                    "category": "Game",
                    "internal_name": "app_456670",
                    "localized_category_name": "游戏",
                    "localized_tag_name": "Hand of Fate 2"
                },
                {
                    "category": "item_class",
                    "internal_name": "item_class_8",
                    "localized_category_name": "物品类型",
                    "localized_tag_name": "个人资料修饰器"
                }
            ],
            "sealed": 0
        },
        {
            "appid": 753,
            "classid": "4681798145",
            "instanceid": "3873495864",
            "currency": 0,
            "background_color": "",
            "icon_url": "a0b44yVZG6xneuQqGEgxNyzAxlmouy8Ipo0bUlJjRDfF9dQS1eD3AHU-t8xnioHyGdqSZpgsmMIM6FMHfQCTW6w-oQcxjhhXynKNPMq4TvfBpzP4_dkHQKfa4-DziMXmrXJUiJR9O40ZcYPp-eqiTNYttHM",
            "icon_url_large": "a0b44yVZG6xneuQqGEgxNyzAxlmouy8Ipo0bUlJjRDfF9dQS1eD3AHU-t8xnioHyGdqSZpgsmMIM6FMHfQCTW6w-oVJlixgFmnyFOM_pRaXDpjzx-IgARfbes-H3jsbirScEi5dyaYwadtfp-eqi10doeW4",
            "descriptions": [
                {
                    "value": ""
                }
            ],
            "tradable": 0,
            "name": "2021 年冬季",
            "type": "2021 年冬季特卖 个人资料修饰器",
            "market_name": "2021 年冬季",
            "market_hash_name": "1846860-Winter 2021",
            "market_fee_app": 1846860,
            "commodity": 1,
            "market_tradable_restriction": 7,
            "market_marketable_restriction": 7,
            "marketable": 0,
            "tags": [
                {
                    "category": "droprate",
                    "internal_name": "droprate_0",
                    "localized_category_name": "稀有度",
                    "localized_tag_name": "普通"
                },
                {
                    "category": "Game",
                    "internal_name": "app_1846860",
                    "localized_category_name": "游戏",
                    "localized_tag_name": "2021 年冬季特卖"
                },
                {
                    "category": "item_class",
                    "internal_name": "item_class_8",
                    "localized_category_name": "物品类型",
                    "localized_tag_name": "个人资料修饰器"
                }
            ],
            "sealed": 0
        },
        {
            "appid": 753,
            "classid": "157508104",
            "instanceid": "246376127",
            "currency": 0,
            "background_color": "",
            "icon_url": "IzMF03bk9WpSBq-S-ekoE33L-iLqGFHVaU25ZzQNQcXdA3g5gMEPvUZZEfSaKqhBT8kyvCOETZfYgdQKwXMa3GdCJiel7nlibuB-fsLLwQL-_OSPSS2jamLFLniOSgg6G-VaNTvYrTCn5O3CRznBFeksQ10FL6FV-mZXfZfeKUBiltNeuVqxmkV6G0t9K5ZCJQnqziRKYuh9nCJEI80DnHDzcMaLhV9nPRc_DrruUr3EPYWljm96CgwhEqLil6BZqg",
            "icon_url_large": "IzMF03bk9WpSBq-S-ekoE33L-iLqGFHVaU25ZzQNQcXdA3g5gMEPvUZZEfSaKqhBT8kyvCOETZfYgdQKwXMa3GdCJiel7nlibuB-fsLLwQL-_OSPSS2jamLFLniOSgg6G-VaNTvYrTCn5O3CRznBFeksQ10FL6FV-mZXfZfeKUBiltNeuVqxmkV6G0t9K5ZCJQnqziRKYuh9nCJEI80DnHDzcMaLhV9nPRc_DrruUr3EPYWljm96CgwhEqLil6BZqg",
            "descriptions": [
                {
                    "value": ""
                }
            ],
            "tradable": 1,
            "name": "Smoker",
            "type": "Left 4 Dead 2 集换式卡牌",
            "market_name": "Smoker",
            "market_hash_name": "550-Smoker",
            "market_fee_app": 550,
            "commodity": 1,
            "market_tradable_restriction": 7,
            "market_marketable_restriction": 7,
            "marketable": 1,
            "tags": [
                {
                    "category": "droprate",
                    "internal_name": "droprate_0",
                    "localized_category_name": "稀有度",
                    "localized_tag_name": "普通"
                },
                {
                    "category": "Game",
                    "internal_name": "app_550",
                    "localized_category_name": "游戏",
                    "localized_tag_name": "Left 4 Dead 2"
                },
                {
                    "category": "cardborder",
                    "internal_name": "cardborder_0",
                    "localized_category_name": "卡牌边框",
                    "localized_tag_name": "普通"
                },
                {
                    "category": "item_class",
                    "internal_name": "item_class_2",
                    "localized_category_name": "物品类型",
                    "localized_tag_name": "集换式卡牌"
                }
            ],
            "sealed": 0
        },
        {
            "appid": 753,
            "classid": "157507746",
            "instanceid": "246376127",
            "currency": 0,
            "background_color": "",
            "icon_url": "IzMF03bk9WpSBq-S-ekoE33L-iLqGFHVaU25ZzQNQcXdA3g5gMEPvUZZEfSaKqhBT8kyvCOETZfYgdQKwXMa3GdCJiel6XVjbqNlNcrBxVisp-GJSCGhOTOSfnSKHgo8S-UKNW_YrGHw4OuVSmzLSOh-EA8DeqJQ7CQXat_bYFBpgcVkomi5kEEgS097dpBAIQvrkydGaekmnicUJJ4GmnKidJTZgAozPRJoU7zmUO7Gb5_lzngyGxdlBkstlc0",
            "icon_url_large": "IzMF03bk9WpSBq-S-ekoE33L-iLqGFHVaU25ZzQNQcXdA3g5gMEPvUZZEfSaKqhBT8kyvCOETZfYgdQKwXMa3GdCJiel6XVjbqNlNcrBxVisp-GJSCGhOTOSfnSKHgo8S-UKNW_YrGHw4OuVSmzLSOh-EA8DeqJQ7CQXat_bYFBpgcVkomi5kEEgS097dpBAIQvrkydGaekmnicUJJ4GmnKidJTZgAozPRJoU7zmUO7Gb5_lzngyGxdlBkstlc0",
            "descriptions": [
                {
                    "value": ""
                }
            ],
            "tradable": 1,
            "name": "Tank",
            "type": "Left 4 Dead 2 集换式卡牌",
            "market_name": "Tank",
            "market_hash_name": "550-Tank",
            "market_fee_app": 550,
            "commodity": 1,
            "market_tradable_restriction": 7,
            "market_marketable_restriction": 7,
            "marketable": 1,
            "tags": [
                {
                    "category": "droprate",
                    "internal_name": "droprate_0",
                    "localized_category_name": "稀有度",
                    "localized_tag_name": "普通"
                },
                {
                    "category": "Game",
                    "internal_name": "app_550",
                    "localized_category_name": "游戏",
                    "localized_tag_name": "Left 4 Dead 2"
                },
                {
                    "category": "cardborder",
                    "internal_name": "cardborder_0",
                    "localized_category_name": "卡牌边框",
                    "localized_tag_name": "普通"
                },
                {
                    "category": "item_class",
                    "internal_name": "item_class_2",
                    "localized_category_name": "物品类型",
                    "localized_tag_name": "集换式卡牌"
                }
            ],
            "sealed": 0
        },
        {
            "appid": 753,
            "classid": "149757868",
            "instanceid": "246376127",
            "currency": 0,
            "background_color": "",
            "icon_url": "IzMF03bk9WpSBq-S-ekoE33L-iLqGFHVaU25ZzQNQcXdA3g5gMEPvUZZEfSaKqhBT8kyvCOETZfYgdQIx3Ma3GdCJielmFE7ILw7feqWhSCt5LvZWVbRLRKRagP4WQdLWMZaIBrR60HX8JiQV0rAVOAlVnwMaKoE52xMK5DUbkVo2YYD_TbqkRUuThB-K5NEJgnsk3JFZe1xynFAJMoEyiD5LpWIgl9kOUB0GuawQO6dLdXw0kB9AEswSq1PMIPAvSToq8WjbPXXJalmYKBkrJeCjwIRGZFFWs08mJ1Fvcn36F4TJZgzHBgKTOszSuGk-Q",
            "icon_url_large": "IzMF03bk9WpSBq-S-ekoE33L-iLqGFHVaU25ZzQNQcXdA3g5gMEPvUZZEfSaKqhBT8kyvCOETZfYgdQIx3Ma3GdCJielmFE7ILw7feqWhSCt5LvZWVbRLRKRagP4WQdLWMZaIBrR60HX8JiQV0rAVOAlVnwMaKoE52xMK5DUbkVo2YYD_TbqkRUuThB-K5NEJgnsk3JFZe1xynFAJMoEyiD5LpWIgl9kOUB0GuawQO6dLdXw0kB9AEswSq1PMIPAvSToq8WjbPXXJalmYKBkrJeCjwIRGZFFWs08mJ1Fvcn36F4TJZgzHBgKTOszSuGk-Q",
            "descriptions": [
                {
                    "value": ""
                }
            ],
            "tradable": 1,
            "name": "无政府主义者",
            "type": "Counter-Strike 2 集换式卡牌",
            "market_name": "无政府主义者",
            "market_hash_name": "730-Anarchist",
            "market_fee_app": 730,
            "commodity": 1,
            "market_tradable_restriction": 7,
            "market_marketable_restriction": 7,
            "marketable": 1,
            "tags": [
                {
                    "category": "droprate",
                    "internal_name": "droprate_0",
                    "localized_category_name": "稀有度",
                    "localized_tag_name": "普通"
                },
                {
                    "category": "Game",
                    "internal_name": "app_730",
                    "localized_category_name": "游戏",
                    "localized_tag_name": "Counter-Strike 2"
                },
                {
                    "category": "cardborder",
                    "internal_name": "cardborder_0",
                    "localized_category_name": "卡牌边框",
                    "localized_tag_name": "普通"
                },
                {
                    "category": "item_class",
                    "internal_name": "item_class_2",
                    "localized_category_name": "物品类型",
                    "localized_tag_name": "集换式卡牌"
                }
            ],
            "sealed": 0
        }
    ],
    "more_items": 1,
    "last_assetid": "17808983937",
    "total_inventory_count": 1025,
    "success": 1,
    "rwgrsn": -2
}
"""u8;
}