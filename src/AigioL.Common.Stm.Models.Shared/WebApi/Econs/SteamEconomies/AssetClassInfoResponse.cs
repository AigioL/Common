namespace AigioL.Common.Stm.Models.WebApi.Econs.SteamEconomies;

public sealed partial record class AssetClassInfoResponse
{
    public AssetClassInfoResponseResult Result { get; set; } = new();

    /// <summary>
    /// <see cref="AssetClassInfoResponse"/> 的 JSON 示例值
    /// </summary>
    public static ReadOnlySpan<byte> ExampleValue =>
"""
{
  "result": {
    "5710094579": {
      "appid": "730",
      "classid": "5710094579",
      "instanceid": "0",
      "icon_url": "i0CoZ81Ui0m-9KwlBY1L_18myuGuq1wfhWSaZgMttyVfPaERSR0Wqmu7LAocGJKz2lu_XsnXwtmkJjSU91dh8bj35VTqVBP4io_frnEVvqf_a6VoIfGSXz7Hlbwg57QwSS_mxhl15jiGyN37c3_GZw91W8BwRflK7EfKsa2sfw",
      "icon_url_large": "",
      "icon_drag_url": "",
      "name": "千瓦武器箱",
      "market_hash_name": "Kilowatt Case",
      "market_name": "千瓦武器箱",
      "name_color": "b0c3d9",
      "background_color": "393b3e",
      "type": "普通级 武器箱",
      "tradable": "1",
      "marketable": "1",
      "commodity": "1",
      "market_tradable_restriction": "7",
      "market_marketable_restriction": "7",
      "descriptions": {
        "0": {
          "type": "html",
          "value": " ",
          "name": "blank"
        },
        "1": {
          "type": "html",
          "value": "这个武器箱中的物品可供租赁。",
          "name": "attr: can open for rental"
        },
        "2": {
          "type": "html",
          "value": "武器箱系列 #393",
          "color": "99ccff",
          "name": "attr: set supply crate series"
        },
        "3": {
          "type": "html",
          "value": " ",
          "name": "blank"
        },
        "4": {
          "type": "html",
          "value": "包含下列物品之一：",
          "name": "attribute"
        },
        "5": {
          "type": "html",
          "value": "双持贝瑞塔 | 藏身处",
          "color": "4b69ff",
          "name": "attribute"
        },
        "6": {
          "type": "html",
          "value": "MAC-10 | 灯箱",
          "color": "4b69ff",
          "name": "attribute"
        },
        "7": {
          "type": "html",
          "value": "新星 | 黑暗徽记",
          "color": "4b69ff",
          "name": "attribute"
        },
        "8": {
          "type": "html",
          "value": "SSG 08 | 灾难",
          "color": "4b69ff",
          "name": "attribute"
        },
        "9": {
          "type": "html",
          "value": "Tec-9 | 渣渣",
          "color": "4b69ff",
          "name": "attribute"
        },
        "10": {
          "type": "html",
          "value": "UMP-45 | 机动化",
          "color": "4b69ff",
          "name": "attribute"
        },
        "11": {
          "type": "html",
          "value": "XM1014 | 刺青",
          "color": "4b69ff",
          "name": "attribute"
        },
        "12": {
          "type": "html",
          "value": "格洛克18型 | 崩络克18型",
          "color": "8847ff",
          "name": "attribute"
        },
        "13": {
          "type": "html",
          "value": "M4A4 | 蚀刻领主",
          "color": "8847ff",
          "name": "attribute"
        },
        "14": {
          "type": "html",
          "value": "FN57 | 混合体",
          "color": "8847ff",
          "name": "attribute"
        },
        "15": {
          "type": "html",
          "value": "MP7 | 笑一个",
          "color": "8847ff",
          "name": "attribute"
        },
        "16": {
          "type": "html",
          "value": "截短霰弹枪 | 模拟输入",
          "color": "8847ff",
          "name": "attribute"
        },
        "17": {
          "type": "html",
          "value": "M4A1消音版 | 黑莲花",
          "color": "d32ce6",
          "name": "attribute"
        },
        "18": {
          "type": "html",
          "value": "宙斯x27电击枪 | 奥林匹斯",
          "color": "d32ce6",
          "name": "attribute"
        },
        "19": {
          "type": "html",
          "value": "USP消音版 | 破颚者",
          "color": "d32ce6",
          "name": "attribute"
        },
        "20": {
          "type": "html",
          "value": "AWP | 镀铬大炮",
          "color": "eb4b4b",
          "name": "attribute"
        },
        "21": {
          "type": "html",
          "value": "AK-47 | 传承",
          "color": "eb4b4b",
          "name": "attribute"
        },
        "22": {
          "type": "html",
          "value": "或一把极为罕见的廓尔喀刀！",
          "color": "ffd700",
          "name": "attribute"
        },
        "23": {
          "type": "html",
          "value": " ",
          "name": "blank"
        },
        "24": {
          "type": "html",
          "value": "",
          "color": "00a000",
          "name": "attribute"
        }
      },
      "owner_descriptions": "",
      "tags": {
        "0": {
          "internal_name": "CSGO_Type_WeaponCase",
          "name": "武器箱",
          "category": "Type",
          "category_name": "类型"
        },
        "1": {
          "internal_name": "set_community_33",
          "name": "千瓦收藏品",
          "category": "ItemSet",
          "category_name": "收藏品"
        },
        "2": {
          "internal_name": "normal",
          "name": "普通",
          "category": "Quality",
          "category_name": "类别"
        },
        "3": {
          "internal_name": "Rarity_Common",
          "name": "普通级",
          "category": "Rarity",
          "color": "b0c3d9",
          "category_name": "品质"
        }
      }
    },
    "success": true
  }
}
"""u8;

    /// <summary>
    /// <see cref="AssetClassInfoResponse"/> 的 JSON [AWP 浮生如梦] 示例值
    /// </summary>
    public static ReadOnlySpan<byte> AWP浮生如梦ExampleValue =>
"""
{
  "result": {
    "2220014235": {
      "appid": "730",
      "classid": "2220014235",
      "instanceid": "0",
      "icon_url": "i0CoZ81Ui0m-9KwlBY1L_18myuGuq1wfhWSaZgMttyVfPaERSR0Wqmu7LAocGIGz3UqlXOLrxM-vMGmW8VNxu5Dx60noTyLwiYbf_jdk7uW-V7R-OfObAXeR1eZJvOhuRz39kE1w4jiAzNiod3qTOgcgXpAlQ-ML5hjqxtHjZOrrtlHWit9EyCj9iDQJsHhCZP-wUg",
      "icon_url_large": "",
      "icon_drag_url": "",
      "name": "AWP | 浮生如梦",
      "market_hash_name": "AWP | Fever Dream (Factory New)",
      "market_name": "AWP | 浮生如梦 (崭新出厂)",
      "name_color": "d32ce6",
      "background_color": "3d293f",
      "type": "保密级 狙击步枪",
      "tradable": "1",
      "marketable": "1",
      "commodity": "0",
      "market_tradable_restriction": "7",
      "market_marketable_restriction": "7",
      "descriptions": {
        "0": {
          "type": "html",
          "value": "外观： 崭新出厂",
          "name": "exterior_wear"
        },
        "1": {
          "type": "html",
          "value": " ",
          "name": "blank"
        },
        "2": {
          "type": "html",
          "value": "高风险，高回报，恶名昭彰的 AWP 因其标志性的枪声和一枪一个的准则而为人熟知。  这把武器在纯黑的底色上使用了粉色、蓝色、以及紫色的定绘。\n\n谵妄为危。",
          "name": "description"
        },
        "3": {
          "type": "html",
          "value": " ",
          "name": "blank"
        },
        "4": {
          "type": "html",
          "value": "光谱收藏品",
          "color": "9da1a9",
          "name": "itemset_name"
        },
        "5": {
          "type": "html",
          "value": " ",
          "name": "blank"
        }
      },
      "owner_descriptions": "",
      "actions": {
        "0": {
          "type": "inspect",
          "name": "在游戏中检视…",
          "link": "steam://rungame/730/76561202255233023/+csgo_econ_action_preview%20S%owner_steamid%A%assetid%D9667761849740989605"
        }
      },
      "market_actions": {
        "0": {
          "type": "inspect",
          "name": "在游戏中检视…",
          "link": "steam://rungame/730/76561202255233023/+csgo_econ_action_preview%20M%listingid%A%assetid%D9667761849740989605"
        }
      },
      "tags": {
        "0": {
          "internal_name": "CSGO_Type_SniperRifle",
          "name": "狙击步枪",
          "category": "Type",
          "category_name": "类型"
        },
        "1": {
          "internal_name": "weapon_awp",
          "name": "AWP",
          "category": "Weapon",
          "category_name": "武器"
        },
        "2": {
          "internal_name": "set_community_16",
          "name": "光谱收藏品",
          "category": "ItemSet",
          "category_name": "收藏品"
        },
        "3": {
          "internal_name": "normal",
          "name": "普通",
          "category": "Quality",
          "category_name": "类别"
        },
        "4": {
          "internal_name": "Rarity_Legendary_Weapon",
          "name": "保密级",
          "category": "Rarity",
          "color": "d32ce6",
          "category_name": "品质"
        },
        "5": {
          "internal_name": "WearCategory0",
          "name": "崭新出厂",
          "category": "Exterior",
          "category_name": "外观"
        }
      }
    },
    "success": true
  }
}
"""u8;

    /// <summary>
    /// <see cref="AssetClassInfoResponse"/> 的 JSON 错误示例值
    /// </summary>
    public static ReadOnlySpan<byte> ErrExampleValue =>
"""
{
    "result": {
        "12345678": {},
        "error": "Unable to get appearance for app 730 classID 12345678  instanceID 0\n",
        "success": false
    }
}
"""u8;
}