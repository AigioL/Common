using System.ComponentModel;

namespace AigioL.Common.AspNetCore.AppCenter.Models;

/// <summary>
/// 用户钱包值类型
/// </summary>
/// <remarks>
/// 目前的理解：
/// - 账号余额：用户的账号余额
///     - 可提现金额：用户的账号余额中可提现的金额
///     - 可用金额：不知道其用途，可理解为“冻结金额”的反面意思？暂且假定值与“账号余额”相同
/// - 免费积分：用户的免费积分
/// - 付费积分：用户的付费积分
///     - 可用付费积分：不知道其用途，可理解为“冻结付费积分”的反面意思？暂且假定值与“付费积分”相同
/// </remarks>
public enum UserWalletValueType : byte
{
    /// <summary>
    /// 账号余额
    /// </summary>
    [Description("账号余额")]
    AccountBalance = 1,

    /// <summary>
    /// 可提现金额
    /// </summary>
    [Description("可提现金额")]
    WithdrawableAmount,

    /// <summary>
    /// 可用金额
    /// </summary>
    [Description("可用金额")]
    AvailableAmount,

    /// <summary>
    /// 免费积分
    /// </summary>
    [Description("免费积分")]
    FreePoints,

    /// <summary>
    /// 付费积分
    /// </summary>
    [Description("付费积分")]
    ProPoints,

    /// <summary>
    /// 可用付费积分
    /// </summary>
    [Description("可用付费积分")]
    AvailableProPoints,
}