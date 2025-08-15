namespace AigioL.Common.AspNetCore.AdminCenter.Models.Users;

/// <summary>
/// 编辑管理后台用户信息模型
/// </summary>
public sealed class EditACUserInfoModel
{
    /// <summary>
    /// 用户名
    /// </summary>
    public required string UserName { get; set; }

    public string? NickName { get; set; }
}
