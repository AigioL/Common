using System.ComponentModel;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

/// <summary>
/// 订单类型
/// </summary>
public enum OrderType : byte
{
    /// <summary>
    /// 一般订单
    /// </summary>
    [Description("一般订单")]
    GeneralOrder = 1,

    /// <summary>
    /// 续费订单
    /// </summary>
    [Description("续费订单")]
    RenewalOrder,
}
