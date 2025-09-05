using System.ComponentModel;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

/// <summary>
/// 商家扣款协议通知状态
/// </summary>
public enum NoticeStatus : byte
{
    /// <summary>
    /// 待通知
    /// </summary>
    [Description("待通知")]
    WaitNotice = 0,

    /// <summary>
    /// 通知失败
    /// </summary>
    [Description("通知失败")]
    NoticeFail = 1,

    /// <summary>
    /// 通知完成
    /// </summary>
    [Description("通知完成")]
    NoticeFinish = 3,
}