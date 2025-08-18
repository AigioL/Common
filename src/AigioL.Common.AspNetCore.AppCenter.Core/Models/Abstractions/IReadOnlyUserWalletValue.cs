namespace AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

/// <summary>
/// 用户钱包值接口
/// </summary>
public interface IReadOnlyUserWalletValue
{
    /// <summary>
    /// 账号余额
    /// </summary>
    decimal AccountBalance { get; }

    /// <summary>
    /// 可提现金额
    /// </summary>
    decimal WithdrawableAmount { get; }

    /// <summary>
    /// 可用金额
    /// </summary>
    decimal AvailableAmount { get; }

    /// <summary>
    /// 付费积分
    /// </summary>
    long ProPoints { get; }

    /// <summary>
    /// 可用付费积分
    /// </summary>
    long AvailableProPoints { get; }

    /// <summary>
    /// 免费积分
    /// </summary>
    long FreePoints { get; }
}

public static partial class UserWalletValueExtensions
{
    /// <summary>
    /// 获取钱包值
    /// </summary>
    /// <param name="i"></param>
    /// <param name="valueType">钱包值类型</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static decimal GetWalletValue(this IReadOnlyUserWalletValue i, UserWalletValueType valueType)
        => valueType switch
        {
            UserWalletValueType.AccountBalance => i.AccountBalance,
            UserWalletValueType.WithdrawableAmount => i.WithdrawableAmount,
            UserWalletValueType.AvailableAmount => i.AvailableAmount,
            UserWalletValueType.FreePoints => i.FreePoints,
            UserWalletValueType.ProPoints => i.ProPoints,
            UserWalletValueType.AvailableProPoints => i.AvailableProPoints,
            _ => throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null),
        };
}