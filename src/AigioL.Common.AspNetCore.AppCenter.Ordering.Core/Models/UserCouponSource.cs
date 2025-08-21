namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

/// <summary>
/// 用户优惠劵来源，用户领到此优惠劵方式
/// </summary>
public enum UserCouponSource : byte
{
    领取 = 1,
    返利,
}
