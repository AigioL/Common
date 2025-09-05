namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;

/// <summary>
/// 会员业务来源
/// </summary>
public enum MembershipBusinessSource : byte
{
    普通订单 = 0,
    CDK激活 = 1,
    协议扣款 = 2,
}
