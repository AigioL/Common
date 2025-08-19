namespace AigioL.Common.AspNetCore.AppCenter.Models;

/// <summary>
/// 会员时长数据修改方向
/// </summary>
public enum MembershipChangeDirection : byte
{
    /// <summary>
    /// 增长
    /// </summary>
    In = 1,

    /// <summary>
    /// 扣除
    /// </summary>
    Out = 2,
}
