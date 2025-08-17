namespace AigioL.Common.AspNetCore.AdminCenter.Models.Users;

public sealed class BMChangePasswordRequest
{
    /// <summary>
    /// 旧密码
    /// </summary>
    public required string OldPassword { get; set; }

    /// <summary>
    /// 新密码
    /// </summary>
    public required string NewPassword { get; set; }
}
