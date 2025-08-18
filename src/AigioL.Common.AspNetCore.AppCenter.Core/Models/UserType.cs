namespace AigioL.Common.AspNetCore.AppCenter.Models;

/// <summary>
/// 用户类型
/// </summary>
[Flags]
public enum UserType : long
{
    /// <summary>
    /// 普通用户
    /// </summary>
    Ordinary = 1 << 0,

    //待定 = 1 << 1,

    /// <summary>
    /// 会员
    /// </summary>
    Membership = 1 << 2,
}
