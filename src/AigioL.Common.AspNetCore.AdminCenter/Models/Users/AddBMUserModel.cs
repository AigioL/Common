namespace AigioL.Common.AspNetCore.AdminCenter.Models.Users;

/// <summary>
/// 新增管理后台用户模型类
/// </summary>
public sealed partial class AddBMUserModel
{
    public required string UserName { get; set; }

    public string? NickName { get; set; }

    public List<string>? Roles { get; set; }

    /// <summary>
    /// 密码，第一次输入
    /// </summary>
    public required string Password1 { get; set; }

    /// <summary>
    /// 密码，第二次输入
    /// </summary>
    public required string Password2 { get; set; }
}
