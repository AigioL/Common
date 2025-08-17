namespace AigioL.Common.AspNetCore.AdminCenter.Models.Users;

/// <summary>
/// 编辑管理后台用户模型类
/// </summary>
public sealed partial class EditBMUserModel
{
    public required string UserName { get; set; }

    public string? NickName { get; set; }

    public List<string>? Roles { get; set; }
}
