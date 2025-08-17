using AigioL.Common.AspNetCore.AdminCenter.Models.Menus;

namespace AigioL.Common.AspNetCore.AdminCenter.Models.Users;

/// <summary>
/// 后台管理用户信息模型
/// </summary>
public sealed partial class BMUserInfoModel
{
    /// <summary>
    /// 用户名
    /// </summary>
    public required string UserName { get; set; }

    public string? NickName { get; set; }

    /// <summary>
    /// 权限角色集合
    /// </summary>
    public List<string>? Roles { get; set; }

    /// <summary>
    /// 头像 Url 地址
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// 是否是管理员
    /// </summary>
    public bool IsAdministrator { get; set; }

    /// <summary>
    /// 菜单列表
    /// </summary>
    public List<BMMenuButtonModel>? Menus { get; set; }

    /// <summary>
    /// 租户 Id
    /// </summary>
    public Guid TenantId { get; set; }
}
