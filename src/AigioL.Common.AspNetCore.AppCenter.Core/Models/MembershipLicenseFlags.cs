using System.ComponentModel;

namespace AigioL.Common.AspNetCore.AppCenter.Models;

/// <summary>
/// 会员订阅类型
/// </summary>
[Flags]
public enum MembershipLicenseFlags
{
    None = 0,

    /// <summary>
    /// 月付
    /// </summary>
    [Description("月付")]
    月付 = 1 << 0,

    /// <summary>
    /// 季付
    /// </summary>
    [Description("季付")]
    季付 = 1 << 1,

    /// <summary>
    /// 年付
    /// </summary>
    [Description("年付")]
    年付 = 1 << 2,

    /// <summary>
    /// 连续包月
    /// </summary>
    [Description("连续包月")]
    连续包月 = 1 << 3,

    /// <summary>
    /// 连续包季
    /// </summary>
    [Description("连续包季")]
    连续包季 = 1 << 4,

    /// <summary>
    /// 连续包年
    /// </summary>
    [Description("连续包年")]
    连续包年 = 1 << 5,

    /// <summary>
    /// CDKey 兑换
    /// </summary>
    [Description("CDKey 兑换")]
    CDKey = 1 << 6,

    /// <summary>
    /// 积分兑换
    /// </summary>
    [Description("积分兑换")]
    Points = 1 << 7,
}