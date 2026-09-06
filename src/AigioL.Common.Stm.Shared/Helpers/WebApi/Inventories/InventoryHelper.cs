using System;
using System.Collections.Generic;
using System.Text;

namespace AigioL.Common.Stm.Helpers.WebApi.Inventories;

/// <summary>
/// Steam 用户库存的帮助类
/// </summary>
public static partial class InventoryHelper
{
    /// <summary>
    /// 模拟 https://steamcommunity.com/id/{0}/inventory 页面行为，第一次调用时传参固定值 75
    /// </summary>
    const uint DefFirstCount = 75;
    const uint DefSecondCount = 2000;
}
