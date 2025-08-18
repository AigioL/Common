using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Models;

/// <summary>
/// 用户钱包余额信息
/// </summary>
/// <param name="AccountBalance">账号余额</param>
/// <param name="WithdrawableAmount">可提现金额</param>
/// <param name="AvailableAmount">可用金额</param>
/// <param name="ProPoints">付费积分</param>
/// <param name="AvailableProPoints">可用付费积分</param>
/// <param name="FreePoints">免费积分</param>
public readonly record struct UserWalletState(
    decimal AccountBalance,
    decimal WithdrawableAmount,
    decimal AvailableAmount,
    long ProPoints,
    long AvailableProPoints,
    long FreePoints) : IReadOnlyUserWalletValue
{
}