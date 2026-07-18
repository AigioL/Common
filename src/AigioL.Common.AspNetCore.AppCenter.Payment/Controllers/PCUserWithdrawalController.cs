using AigioL.Common.AspNetCore.AppCenter.Payment.Models;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Controllers;

/// <summary>
/// PC 用户提现控制器
/// </summary>
public static partial class PCUserWithdrawalController
{
    /// <summary>
    /// 映射 PC 用户提现相关端点
    /// </summary>
    public static void MapPCUserWithdrawalEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/pc/withdrawal");

        // 申请提现
        group.MapPost("/apply", static async (
            [FromBody] PCUserWithdrawalRequestModel request,
            [FromServices] IPCUserWithdrawalService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ApplyWithdrawalAsync(request, cancellationToken);
            return result.IsSuccess() ? Results.Ok(result) : Results.BadRequest(result);
        });

        // 查询钱包信息
        group.MapGet("/wallet/{userId:guid}", static async (
            [FromRoute] Guid userId,
            [FromServices] IPCUserWithdrawalService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetWalletInfoAsync(userId, cancellationToken);
            return result.IsSuccess() ? Results.Ok(result) : Results.NotFound(result);
        });

        // 查询提现记录
        group.MapGet("/record/{withdrawalNumber}", static async (
            [FromRoute] string withdrawalNumber,
            [FromServices] IPCUserWithdrawalService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetWithdrawalAsync(withdrawalNumber, cancellationToken);
            return result.IsSuccess() ? Results.Ok(result) : Results.NotFound(result);
        });
    }
}
