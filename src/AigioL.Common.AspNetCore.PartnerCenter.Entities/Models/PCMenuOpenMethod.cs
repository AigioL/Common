namespace AigioL.Common.AspNetCore.PartnerCenter.Models;

/// <summary>
/// 合作伙伴后台菜单的打开方式
/// </summary>
public enum PCMenuOpenMethod : byte
{
    /// <summary>
    /// 正常方式，在页面中打开
    /// </summary>
    Normal = 0,

    /// <summary>
    /// 打开链接
    /// </summary>
    OpenLink = 1,
}
