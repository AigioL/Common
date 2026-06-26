using AigioL.Common.AspNetCore.AppCenter.Payment.Models;
using AigioL.Common.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;

/// <summary>
/// PC 用户提现服务
/// </summary>
public partial interface IPCUserWithdrawalService
{
    /// <summary>
    /// 申请提现
    /// </summary>
    /// <param name="request">提现请求</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRsp<PCUserWithdrawalResponseModel>> ApplyWithdrawalAsync(
        PCUserWithdrawalRequestModel request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询钱包信息
    /// </summary>
    /// <param name="userId">用户 Id</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRsp<PCUserWalletInfoModel>> GetWalletInfoAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询提现记录
    /// </summary>
    /// <param name="withdrawalNumber">提现单号</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRsp<PCUserWithdrawalResponseModel>> GetWithdrawalAsync(
        string withdrawalNumber,
        CancellationToken cancellationToken = default);
}
