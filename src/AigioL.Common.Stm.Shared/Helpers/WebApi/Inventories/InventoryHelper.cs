namespace AigioL.Common.Stm.Helpers.WebApi.Inventories;

/// <summary>
/// Steam 用户库存的帮助类
/// </summary>
public static partial class InventoryHelper
{
    /// <summary>
    /// 模拟 https://steamcommunity.com/id/{0}/inventory 页面行为，第一次调用时传参固定值 75
    /// </summary>
    public const uint DefFirstCount = 75;

    /// <summary>
    /// 模拟 https://steamcommunity.com/id/{0}/inventory 页面行为，第二次及之后调用时传参固定值 2000
    /// </summary>
    public const uint DefSecondCount = 2000;
}
