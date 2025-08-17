namespace AigioL.Common.AspNetCore.AdminCenter.Models;

/// <summary>
/// 管理后台菜单的打开方式
/// </summary>
public enum BMMenuOpenMethod : byte
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
