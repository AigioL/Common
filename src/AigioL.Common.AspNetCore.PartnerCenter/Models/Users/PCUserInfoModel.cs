using AigioL.Common.AspNetCore.AdminCenter.Models.Users;
using AigioL.Common.AspNetCore.PartnerCenter.Models.Menus;

namespace AigioL.Common.AspNetCore.PartnerCenter.Models.Users;

public sealed partial class PCUserInfoModel
{
    public const string DefaultAvatarUrl = BMUserInfoModel.DefaultAvatarUrl;

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
    public string? Avatar { get; set; } = DefaultAvatarUrl;

    /// <summary>
    /// 是否是管理员
    /// </summary>
    public bool IsAdministrator { get; set; }

    /// <summary>
    /// 菜单列表
    /// </summary>
    public List<PCMenuButtonModel>? Menus { get; set; }

    /// <summary>
    /// 租户 Id
    /// </summary>
    public Guid TenantId { get; set; }
}