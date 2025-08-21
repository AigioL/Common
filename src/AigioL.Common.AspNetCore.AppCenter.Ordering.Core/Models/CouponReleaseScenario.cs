namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

/// <summary>
/// 优惠劵的发行场景
/// </summary>
public enum CouponReleaseScenario : byte
{
    免费领取 = 1,
    关联赠送,
    积分兑换,
    真实购买,
}