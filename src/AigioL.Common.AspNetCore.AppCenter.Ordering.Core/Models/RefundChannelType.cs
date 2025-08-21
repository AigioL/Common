using System.ComponentModel;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

/// <summary>
/// 退款方式类型
/// </summary>
public enum RefundChannelType
{
    /// <summary>
    /// 原路支付平台退回
    /// </summary>
    [Description("原路支付平台退回")]
    OriginalWay = 1,

    /// <summary>
    /// 人工处理
    /// </summary>
    [Description("人工处理")]
    ManualHandling = 2,
}