using AigioL.Common.AspNetCore.PartnerCenter.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Models;

/// <summary>
/// PC 用户提现请求模型
/// </summary>
public sealed class PCUserWithdrawalRequestModel
{
    /// <summary>
    /// PC 用户 Id
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 提现金额（元）
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 微信 OpenId（用于微信转账）
    /// </summary>
    public string? WeChatOpenId { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Note { get; set; }
}

/// <summary>
/// PC 用户提现响应模型
/// </summary>
public sealed class PCUserWithdrawalResponseModel
{
    /// <summary>
    /// 提现单号
    /// </summary>
    public string WithdrawalNumber { get; init; } = null!;

    /// <summary>
    /// 提现金额
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// 提现状态
    /// </summary>
    public PCUserWithdrawalStatus Status { get; init; }
}

/// <summary>
/// PC 用户钱包信息模型
/// </summary>
public sealed class PCUserWalletInfoModel
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// 可提现金额
    /// </summary>
    public decimal WithdrawableAmount { get; init; }

    /// <summary>
    /// 已提现金额
    /// </summary>
    public decimal WithdrawnAmount { get; init; }

    /// <summary>
    /// 累计结算金额
    /// </summary>
    public decimal CumulativeSettlementAmount { get; init; }
}
