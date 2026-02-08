using System.ComponentModel;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.Statistics;

/// <summary>
/// 统计用户数量
/// </summary>
public enum StatisticUserCount : byte
{
    [Description("用户总数")]
    UserCount = 0,

    #region 绑定方式

    [Description("手机号码")]
    BindPhone = 1,

    [Description("邮箱")]
    BindEmail = 2,

    [Description("Steam")]
    BindSteam = 11,

    [Description("QQ")]
    BindQQ = 12,

    [Description("Microsoft")]
    BindMS = 13,

    [Description("支付宝")]
    BindAlipay = 14,

    [Description("微信")]
    BindWeChat = 15,

    #endregion
}